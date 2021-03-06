using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Toshka.dbgSave.DataAccess;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
using System.IO;
using Toshka.dbgSave.Model;
using System.Data;
using Toshka.dbgSave.Services.Abstraction;
using Telegram.Bot;
using System.Net.Http.Headers;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.Structure;
using Toshka.dbgSave.Helpers;
using System.Drawing.Imaging;
using Toshka.dbgSave.NeuralNetwork.Events;
using Toshka.dbgSave.NeuralNetwork.ProcessVideo;
using Microsoft.AspNetCore.SignalR;
using Toshka.dbgSave.Hub;
using Toshka.dbgSave.NeuralNetwork.DetectorTools;
using Toshka.dbgSave.NeuralNetwork.DetectorTools.Tracker;

namespace Toshka.dbgSave.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CameraController : ControllerBase
    {
        private Event DangerEvent;
        private bool DangerEventCountFrameFlag;
        private int NotDangerEventCountFrame;
        private int DangerEventCountFrame;

        private Rectangle DangerEventObjectRect;

        private readonly IHubContext<DetectionHub> _hubContext;
        private readonly PointsConverter _converter = new PointsConverter();
        private readonly IBotService _botService;
        private readonly EfContext _context;
        private readonly ICameraService _cameraService;
        public CameraController(IBotService botService, IHubContext<DetectionHub> hubContext,
        EfContext context, ICameraService cameraService)
        {
            _hubContext = hubContext;
            _botService = botService;
            _context = context;
            _cameraService = cameraService;
        }

        [HttpGet]
        public JsonResult Index()
        {
            return new JsonResult(_cameraService.GetAllCameras());
        }

        [HttpPut]
        [Route("update")]
        public StatusCodeResult Update([FromBody] Camera camera)
        {
            _cameraService.Update(camera);

            return Ok();
        }

        [HttpGet]
        [Route("video/{id?}")]
        public IActionResult CameraVideo(long id)
        {
            //todo retrive video from camera
            if (TryOpenFile("test.avi", FileMode.Open, out FileStream fs))
            {
                FileStreamResult result = File(
                    fileStream: fs,
                    contentType: new MediaTypeHeaderValue("video/mp4").MediaType,
                    enableRangeProcessing: true //<-- enable range requests processing,

                );
                return result;
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("lastdetection/{id?}")]
        public IActionResult LastDetection(long id)
        {
            try
            {
                Camera camera = _cameraService.GetById(id);

                VideoCapture videoCapture =
                    new VideoCapture(System.IO.Path.Combine(Environment.CurrentDirectory, "test.avi"));
                int frame = 0;
                this.DangerEventCountFrame = 0;
                this.NotDangerEventCountFrame = 0;
                Bitmap previousFrame = null;
                while (videoCapture.Grab())
                {
                    frame++;

                    if (this.DangerEventCountFrameFlag == false)
                    {
                        this.NotDangerEventCountFrame++;
                    }
                    else
                    {
                        this.NotDangerEventCountFrame = 0;
                    }

                    if (this.NotDangerEventCountFrame >= 3)
                    {
                        //TODO:
                        if (this.DangerEvent != null)
                        {
                            this.DangerEvent.EndTime = DateTime.Now;
                        }
                    }

                    Image<Bgr, byte> image = videoCapture.QueryFrame().ToImage<Bgr, byte>();
                    Bitmap bmp = new Bitmap(image.ToBitmap());
                    if (previousFrame != null)
                    {
                        Bitmap frameProcess = FrameProcess(bmp, previousFrame, frame, camera);
                        if (frameProcess != null)
                        {
                            MemoryStream stream = frameProcess.ToStream(ImageFormat.Jpeg);
                            string base64String = Convert.ToBase64String(stream.ToArray());

                            if (this.DangerEvent != null)
                            {
                                this.DangerEvent.EndTime = DateTime.Now;
                                _context.Events.Add(this.DangerEvent);
                                _context.SaveChanges();
                            }
                            return new JsonResult(base64String);
                            FileStreamResult result = File(
                                fileStream: stream,
                                contentType: new MediaTypeHeaderValue("image/jpeg").MediaType,
                                enableRangeProcessing: true //<-- enable range requests processing,

                            );
                            return result;
                        }
                    }

                    previousFrame = bmp;
                    //bmp.Save(frame + ".bmp");
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("cameravideo/{id?}")]
        public IActionResult Video(long id)
        {
            try
            {
                Camera camera = _cameraService.GetById(id);

                VideoCapture videoCapture =
                    new VideoCapture(System.IO.Path.Combine(Environment.CurrentDirectory, "many_people1.mp4"));//"test.avi"));
                int frame = 0;
                Bitmap previousFrame = null;
                while (videoCapture.Grab())
                {
                    frame++;

                    Image<Bgr, byte> image = videoCapture.QueryFrame().ToImage<Bgr, byte>();
                    Bitmap bmp = new Bitmap(image.ToBitmap());
                    if (previousFrame != null)
                    {
                        FrameProcess(bmp, previousFrame, frame, camera);
                    }

                    previousFrame = bmp;
                    //bmp.Save(frame + ".bmp");
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }

            return BadRequest();
        }

        private static bool TryOpenFile(string file, FileMode open, out FileStream fileStream)
        {
            fileStream = System.IO.File.Open(file, open);
            if (fileStream != null)
            {
                return true;
            }

            return false;
        }

        private Bitmap FrameProcess(Bitmap image, Bitmap previousImage, int currentFrame, Camera camera)
        {
            image = FilterImage(image, camera);
            image = ColorImprove(image, camera);

            var _detectorMatches = new List<DetectorMatch>();
            var _neuralDetector =
                new NeuralNetwork.DetectorTools.NeuralNetwork(System.IO.File.ReadAllBytes(System.IO.Path.Combine(Environment.CurrentDirectory, "model.pb")));
            //НЕЙРОННЫЕ СЕТИ
            if (previousImage != null)
                _detectorMatches.Add(_neuralDetector.DetectObjects(image, previousImage, currentFrame, 1));

            var detectedObjects = _neuralDetector.DetectedObjects.Where(o => o.ClassId != 5).ToList();

            #region Tracking

            // Пересечение ограничивающих прямоугольников - мусора и мусорных баков
            calculateGarbageWithTrashIntersection(detectedObjects, camera);

            // Подсчет трафика
            calculateTraffic(detectedObjects);

            // Фильтрация зон
            filterDetectedObjects(detectedObjects, camera);

            // Пересечение машин с газонами
            detectIllegalParking(detectedObjects, camera);

            //if (_detectorMatches.Any())
            //{
            //    _emissionEvents = _objectTracker.GetTrackMatchEvents(_detectorMatches.Last(), _shapes, _emissionEvents,
            //        _player.currentFrameNo, camera);
            //}

            #endregion

            //TODO: Рассчет дистанции до события
            // DetectedObject bottomEventObject = detectedObjects
            //     .OrderByDescending(d => d.Rectangle.Bottom).FirstOrDefault();

            calculateEventDistance(camera, detectedObjects, image);
            return DrawObjects(image, detectedObjects, camera);
        }

        // Расчет заполненности мусорного бака
        private void calculateGarbageWithTrashIntersection(List<DetectedObject> detectedObjects, Camera camera)
        {
            List<DetectedObject> trashObjects = detectedObjects.FindAll(detectedObject => detectedObject.ClassId == 11 || detectedObject.ClassId == 12);
            if (trashObjects.Count != 0)
            {
                List<DetectedObject> garbages = detectedObjects.FindAll(detectedObject => detectedObject.ClassId == 10);
                foreach (DetectedObject garbage in garbages)
                {
                    foreach (DetectedObject trash in trashObjects)
                    {
                        if (TrackerUtils.IsIntersectDetectedObjects(garbage, trash))
                        {
                            if (garbage.Rectangle.Top < trash.Rectangle.Top)
                            {
                                Event evFullness = new Event();
                                evFullness.CameraId = camera.Id;
                                evFullness.StartTime = DateTime.Now;
                                _context.Events.Add(evFullness);
                                _context.SaveChanges();


                            }
                        }
                    }
                }
            }
        }

        // Подсчет пешеходного и транспортного трафика для будущего анализа
        private void calculateTraffic(List<DetectedObject> detectedObjects)
        {
            int numberOfPeople = detectedObjects.FindAll(detectedObject => detectedObject.ClassId == 6).Count;
            int numberOfCars = detectedObjects.FindAll(detectedObject => detectedObject.ClassId == 5).Count;
            // toDo : используем ключевые точки, чтобы посчитать количество уникальных объектов
        }


        //Фильтрация обнаруженных областей с дымом (пересечение с разметкой - техногенными объектами, дым с которых анализировать не нужно)
        private List<DetectedObject> filterDetectedObjects(List<DetectedObject> detectedObjects, Camera camera)
        {
            List<DetectedObject> filterdObjects = new List<DetectedObject>();

            List<CameraFilterMarkup> shapes = _context.CameraFilterMarkups.Where(e => e.CameraId == camera.Id && e.forCar == false).OrderBy(b => b.CameraId).ToList();
            List<Shape> filterShapes = new List<Shape>();

            foreach (var sshape in shapes)
            {
                Shape shape = new Shape();

                PointF pointLeftDown = new PointF();
                pointLeftDown.X = sshape.dlx;
                pointLeftDown.Y = sshape.dly;

                PointF pointLeftUp = new PointF();
                pointLeftUp.X = sshape.ulx;
                pointLeftUp.Y = sshape.uly;

                PointF pointRightUp = new PointF();
                pointRightUp.X = sshape.urx;
                pointRightUp.Y = sshape.ury;

                PointF pointRightDown = new PointF();
                pointRightDown.X = sshape.drx;
                pointRightDown.Y = sshape.dry;

                shape.Points.AddRange(new[] { pointRightDown, pointRightUp, pointLeftUp, pointLeftDown });
                filterShapes.Add(shape);
            }

            foreach (DetectedObject detectedObject in detectedObjects)
            {
                Shape matchedShape = TrackerUtils.FindMatchedShape(detectedObject, filterShapes, 0.2);
                if (matchedShape == null)
                {
                    filterdObjects.Add(detectedObject);
                }
            }

            return filterdObjects;
        }

        // Обнаружение неправильной парковки (на газонах и т.д. Для этого используем разметку)
        private void detectIllegalParking(List<DetectedObject> detectedObjects, Camera camera)
        {
            List<DetectedObject> cars = detectedObjects.FindAll(detectedObject => detectedObject.ClassId == 5);

            List<CameraFilterMarkup> shapes = _context.CameraFilterMarkups.Where(e => e.CameraId == camera.Id && e.forCar == true).OrderBy(b => b.CameraId).ToList();
            List<Shape> illegalParkingShapes = new List<Shape>();

            foreach (var sshape in shapes)
            {
                Shape shape = new Shape();

                PointF pointLeftDown = new PointF();
                pointLeftDown.X = sshape.dlx;
                pointLeftDown.Y = sshape.dly;

                PointF pointLeftUp = new PointF();
                pointLeftUp.X = sshape.ulx;
                pointLeftUp.Y = sshape.uly;

                PointF pointRightUp = new PointF();
                pointRightUp.X = sshape.urx;
                pointRightUp.Y = sshape.ury;

                PointF pointRightDown = new PointF();
                pointRightDown.X = sshape.drx;
                pointRightDown.Y = sshape.dry;

                shape.Points.AddRange(new[] { pointRightDown, pointRightUp, pointLeftUp, pointLeftDown });
                illegalParkingShapes.Add(shape);
            }

            foreach (DetectedObject detectedObject in cars)
            {
                Shape matchedShape = TrackerUtils.FindMatchedShape(detectedObject, illegalParkingShapes, 0.5);
                if (matchedShape != null)
                {
                    Event evIllegalParking = new Event();
                    evIllegalParking.CameraId = camera.Id;
                    evIllegalParking.StartTime = DateTime.Now;
                    _context.Events.Add(evIllegalParking);
                    _context.SaveChanges();
                }
            }

        }

        // Расчет расстояния до объектов
        private void calculateEventDistance(Camera camera, List<DetectedObject> detectedObjects, Bitmap image)
        {
            // distance to object (mm) = (focal length (mm) * real height of the object (mm) * image height (pixels)) / (object height (pixels) * sensor height (mm))

            // ToDo : калибровка камеры, ее реальное фокусное расстояние и размер матрицы

            foreach (DetectedObject detectedObject in detectedObjects) {
                double realObjectHeight = 0;
                if (detectedObject.ClassId == 5 || detectedObject.ClassId == 6)
                {
                    realObjectHeight = 1700;
                } else
                {
                    return;
                }

                double focalLength = camera.focalLength.Equals(0) ? 2.8 : camera.focalLength;
                double imageSensorSize = camera.imageSensorSize.Equals(0) ? 3.63 : camera.imageSensorSize;

                double distance = (focalLength * realObjectHeight * image.Height) / (detectedObject.Rectangle.Height * imageSensorSize);
            }
        }

        private Dictionary<int, string> _colorGeneralClasses = new Dictionary<int, string>
        {
            // Общая
            [1] = "#ff3300",//"Дым",
            [2] = "#ff3300",//"Дым",
            [3] = "#ff3300",//"Дым",
            [4] = "#ff0000",//"Огонь",
            [5] = "#00cc00",//"Автомобиль",
            [6] = "#00cc66",//"Человек",
            [7] = "#e60000",//"Оружие",
            [8] = "#e60000",//"Нож",
            [9] = "#00ff99",//"Граффити",
            [10] = "#33ff77",//"Мусорный бак",
            [11] = "#66ff99",//"Мусорный пакет",
            [12] = "#4dff88",//"Мусор",
            [13] = "#ff8080"//"Собака"
        };

        private Dictionary<int, string> _colorTrashTypeClasses = new Dictionary<int, string>
        {
            // Типы мусора
            [1] = "#0066ff",//"Пластик",
            [2] = "#4d94ff",//"Бумага",
            [3] = "#99c2ff",//"Стекло",
            [4] = "#005ce6",//"Металл"
        };

        private Dictionary<int, string> _colorMaskClasses = new Dictionary<int, string>
        {
            // Маски
            [1] = "#33cc33",//"С маской",
            [2] = "#55ff00",//"Без маски",
            [3] = "#00cc66"//"Неправильно надета"
        };

        private Bitmap DrawObjects(Bitmap image, IEnumerable<DetectedObject> detectedObjects, Camera camera)
        {
            this.DangerEventCountFrameFlag = false;

            using (Graphics g = Graphics.FromImage(image))
            {

                List<DetectedObject> selectedDetectedObjects = new List<DetectedObject>();

                int dogCount = 0;
                string type;

                foreach (var detectedObject in detectedObjects)
                {
                    var color = ColorTranslator.FromHtml(_colorGeneralClasses[detectedObject.ClassId]);
                    Rectangle box = _converter.ConvertRectangleToRelativeSize(detectedObject.Rectangle);
                    var lineWidth = 1.5f;

                    g.DrawRectangle(new Pen(color, (float)lineWidth), box);

                    Point textPosition = new Point(box.X, box.Y - 15);
                    //Size sizeOfText = TextRenderer.MeasureText(detectedObject.ClassName, new Font("Arial", 12f));
                    //Rectangle rect = new Rectangle(textPosition, 12f);
                    //g.FillRectangle(Brushes.Black, rect);
                    //g.DrawString(detectedObject.ClassName, new Font("Arial", 12f), Brushes.White, textPosition);
                    //var color = Color.Green;

                    if (detectedObject.ClassId == 1 || detectedObject.ClassId == 2 || detectedObject.ClassId == 3 ||
                        detectedObject.ClassId == 4 || detectedObject.ClassId == 7 || detectedObject.ClassId == 8 ||
                        detectedObject.ClassId == 9)
                    {
                        color = Color.Red;
                    } else if(detectedObject.ClassId == 12)
                    {
                        color = Color.Yellow;
                    } 

                    //Стая
                    if (detectedObject.ClassId == 13)
                    {
                        dogCount++;
                        if (dogCount >= 4)
                        {
                            this.DangerEventCountFrameFlag = true;
                            type = "dogs";
                        }
                    }

                    //Rectangle box = _converter.ConvertRectangleToRelativeSize(detectedObject.Rectangle);
                    //var lineWidth = 6f * detectedObject.Density;
                    //lineWidth = lineWidth == 0 ? 2f : lineWidth;
                    if (selectedDetectedObjects.Contains(detectedObject))
                    {
                        using (Brush fillbrush = new SolidBrush(Color.FromArgb(80, color)))
                        {
                            g.FillRectangle(fillbrush, box);
                        }
                    }

                    //TODO: подпись rectangle
                    //string caption = detectedObject.ClassId == 4 ? "огонь" : "дым";

                    g.DrawRectangle(new Pen(color, (float)lineWidth), box);
                    //image.Save(Guid.NewGuid().ToString() + ".bmp");
                    var imageMessage = new ImageMessage
                    {
                        ImageHeaders = "data:" + "img" + ";base64,",
                        ImageBase64 = Convert.ToBase64String(
                            image.ToStream(ImageFormat.Jpeg).ToArray())
                    };

                    //TODO: алгоритм подсчета количества людей для определения аномалий  и добавить в danger event

                    //danger event
                    if (detectedObject.ClassId == 1 || detectedObject.ClassId == 2 || detectedObject.ClassId == 3 ||
                        detectedObject.ClassId == 4 || detectedObject.ClassId == 7 || detectedObject.ClassId == 8 ||
                        detectedObject.ClassId == 9)
                    {
                        //Вычисляем направление движения для человека с оружием относительно смещения rectangle на изображении
                        if (detectedObject.ClassId == 7 || detectedObject.ClassId == 8 
                            && DangerEventObjectRect != null)
                        {
                            type = "gun";
                            int direct = GetDirectionForDangerEvent(detectedObjects.First().Rectangle);
                            camera.PeopleDeg = direct.ToString();
                        }

                        //Огонь/Дым
                        if (detectedObject.ClassId == 1 || detectedObject.ClassId == 2 || 
                            detectedObject.ClassId == 3 || detectedObject.ClassId == 4)
                        {
                            type = "fire";
                        }

                        this.DangerEventCountFrame++;
                        this.DangerEventCountFrameFlag = true;

                        //TODO: определить для события службу реагирования
                        _hubContext.Clients.All.SendAsync("ondetected", imageMessage);
                        
                        //foreach (var user in _context.TelegramUsers)
                        //{
                        //    _botService.Client.SendPhotoAsync(user.ChatId, new Telegram.Bot.Types.InputFiles.InputOnlineFile(image.ToStream(ImageFormat.Jpeg)), $"Обнаруженено опасное событие в координатах: {camera.Latitude}\"N {camera.Longitude}\"E. Время: {DateTime.Now:dd.MM.yyyy HH:mm}").GetAwaiter().GetResult();
                        //    _botService.Client.SendLocationAsync(user.ChatId, (double)camera.Latitude, (double)camera.Longitude).GetAwaiter().GetResult();
                        //}
                    }

                    if (this.DangerEvent == null && this.DangerEventCountFrameFlag == true)
                    {
                        DangerEventObjectRect = detectedObjects.First().Rectangle;

                        this.DangerEvent = new Event();
                        DangerEvent.CameraId = camera.Id;
                        DangerEvent.StartTime = DateTime.Now;
                        camera.Type = "red";
                    }

                    _cameraService.Update(camera);

                    break;
                    //var brush = new SolidBrush(color);
                    //DrawName(g, new PointF(box.X, box.Y), detectedObject.ClassName, brush);
                }

                return image;
            }
        }

        private int GetDirectionForDangerEvent(Rectangle object_rect_new)
        {
            //TODO: using key points lib (sift/surf) for eq objects

                bool up = false;
                bool down = false;
                bool right = false;
                bool left = false;
                bool x_stay = false;
                bool y_stay = false;

                int directing = 0;

                if (DangerEventObjectRect.Y > object_rect_new.Y)
                {
                    //Вверх
                    up = true;
                }

                if (DangerEventObjectRect.Y < object_rect_new.Y)
                {
                    //Вниз
                    down = true;
                }

                if (DangerEventObjectRect.X > object_rect_new.X)
                {
                    //Вправо
                    right = true;
                }

                if (DangerEventObjectRect.X < object_rect_new.X)
                {
                    //Влево
                    left = true;
                }

                if (DangerEventObjectRect.X == object_rect_new.X)
                {
                    //По горизонтали стоит
                    x_stay = true;
                }

                if (DangerEventObjectRect.Y == object_rect_new.Y)
                {
                    //По Вертикали стоит
                    y_stay = true;
                }

                if (up == true && down == false && left == false && right == false)
                {
                    directing = 1;
                }

                if (up == false && down == true && left == false && right == false)
                {
                    directing = 2;
                }

                if (up == false && down == false && left == true && right == false)
                {
                    directing = 3;
                }

                if (up == false && down == false && left == false && right == true)
                {
                    directing = 4;
                }

                if (up == true && down == false && left == true)
                {
                    //Северо-запад
                    directing = 5;
                }

                if (up == true && down == false && right == true)
                {
                    //Северо-восток
                    directing = 6;
                }

                if (up == false && down == true && left == true)
                {
                    //Юго-запад
                    directing = 7;
                }

                if (up == false && down == true && right == true)
                {
                    //Юго-восток
                    directing = 8;
                }

            return directing;
        }

        private Bitmap FilterImage(Bitmap image, Camera camera)
        {
            //TODO: filter noise

            return image;
        }
        private int GetSettingsValue(string input, int defaultValue = 0)
        {
            if (int.TryParse(input, out int bright))
            {
                defaultValue = bright;
            }
            return defaultValue;
        }

        private Bitmap ColorImprove(Bitmap image, Camera camera)
        {
            //TODO: camera settings

            return image;
        }
    }
}
