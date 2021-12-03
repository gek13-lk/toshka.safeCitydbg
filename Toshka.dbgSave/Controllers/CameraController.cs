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
                _detectorMatches.Add(_neuralDetector.DetectObjects(image, previousImage, currentFrame));

            var detectedObjects = _neuralDetector.DetectedObjects.Where(o => o.ClassId != 5).ToList();

            #region Tracking

            //Фильтрация зон
            //filterDetectedObjects(detectedObjects, camera);

            //if (_detectorMatches.Any())
            //{
            //    _emissionEvents = _objectTracker.GetTrackMatchEvents(_detectorMatches.Last(), _shapes, _emissionEvents,
            //        _player.currentFrameNo, camera);
            //}

            #endregion

            //TODO: Рассчет дистанции до события
            DetectedObject bottomEventObject = detectedObjects
                .OrderByDescending(d => d.Rectangle.Bottom).FirstOrDefault();

            return DrawObjects(image, detectedObjects, camera);
        }

        //TODO: filter zone 
        private List<DetectedObject> filterDetectedObjects(List<DetectedObject> detectedObjects, Camera camera)
        {
            List<DetectedObject> filterdObjects = new List<DetectedObject>();

            return filterdObjects;
        }

        //TODO: distance
        /*private Emgu.CV.Structure.Range calculateEventDistance(Camera camera, DetectedObject detectedObject)
        {
            
            return 0;
        }*/

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
                    var color = Color.Green;

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

                    Rectangle box = _converter.ConvertRectangleToRelativeSize(detectedObject.Rectangle);
                    var lineWidth = 6f * detectedObject.Density;
                    lineWidth = lineWidth == 0 ? 2f : lineWidth;
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
