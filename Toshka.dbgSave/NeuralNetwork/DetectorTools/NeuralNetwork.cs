using System;
using System.Collections.Generic;
using System.Drawing;
using TensorFlow;
using Toshka.dbgSave.NeuralNetwork.Events;
using Emgu.CV;
using Emgu.CV.Structure;
using Toshka.dbgSave.NeuralNetwork.DetectorTools.Tracker;

namespace Toshka.dbgSave.NeuralNetwork.DetectorTools
{
    public class NeuralNetwork
    {
        public List<DetectedObject> DetectedObjects { get; set; }

        private TFGraph _graph;
        private TFSession _session;
        private readonly object _sessionLocker = new object();
        private int _minIntersectPercentage = 80;

        private Dictionary<int, string> _nameGeneralClasess = new Dictionary<int, string>
        {
            // Общая
            [1] = "Дым",
            [2] = "Дым",
            [3] = "Дым",
            [4] = "Огонь",
            [5] = "Автомобиль",
            [6] = "Человек",
            [7] = "Оружие",
            [8] = "Нож",
            [9] = "Граффити",
            [10] = "Мусорный бак",
            [11] = "Мусорный пакет",
            [12] = "Мусор",
            [13] = "Собака"

            // Типы мусора
            /* [1] = "Пластик",
             [2] = "Бумага",
             [3] = "Стекло",
             [4] = "Металл"*/

            // Маски
            /*[1] = "С маской",
            [2] = "Без маски",
            [3] = "Неправильно надета"*/
        };

        private Dictionary<int, string> _nameTrashTypeClasess = new Dictionary<int, string>
        {
            // Типы мусора
             [1] = "Пластик",
             [2] = "Бумага",
             [3] = "Стекло",
             [4] = "Металл"
        };

        private Dictionary<int, string> _nameMaskClasess = new Dictionary<int, string>
        {
            // Маски
            [1] = "С маской",
            [2] = "Без маски",
            [3] = "Неправильно надета"
        };

        public class ImageFrame
        {
            public byte[] Bytes { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public NeuralNetwork(byte[] model)
        {
            InitSession(model);
            DetectedObjects = new List<DetectedObject>();
        }

        private void InitSession(byte[] model)
        {
            _graph = ImportGraph(model);
            _session = CreateSession(_graph);
        }

        private static TFSession CreateSession(TFGraph graph)
        {
            return new TFSession(graph);
        }

        private static TFGraph ImportGraph(byte[] model)
        {
            var graph = new TFGraph();
            graph.Import(model);
            return graph;

        }

        public DetectorMatch DetectObjects(Bitmap frameNow, Bitmap framePrev, int numberFrame, int typeNetwork)
        {
            DetectedObjects = new List<DetectedObject>();

            Image<Bgr, Byte> img = frameNow.ToImage<Bgr, byte>();
            Mat matNow = img.Mat;

            img = framePrev.ToImage<Bgr, byte>();
            Mat matPrev = img.Mat;

            ImageFrame imageNow = new ImageFrame
            {
                Width = matNow.Width,
                Height = matNow.Height,
                Bytes = matNow.GetRawData()
            };

            ImageFrame imagePrev = new ImageFrame
            {
                Width = matPrev.Width,
                Height = matPrev.Height,
                Bytes = matPrev.GetRawData()
            };

            var tensor = CreateTensorFromImageFrame(imageNow);

            TFTensor[] output;
            lock (_sessionLocker)
            {
                output = _session.Run(new[] { _graph["image_tensor"][0] }, new[] { tensor }, new[]
                {
                    _graph["detection_boxes"][0],
                    _graph["detection_scores"][0],
                    _graph["detection_classes"][0],
                    _graph["num_detections"][0]
                });
            }

            var boxes = (float[,,])output[0].GetValue(jagged: false);
            var scores = (float[,])output[1].GetValue(jagged: false);
            var classes = (float[,])output[2].GetValue(jagged: false);

            var x = boxes.GetLength(0);
            var y = boxes.GetLength(1);

            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    var classIndex = Convert.ToInt32(classes[i, j]);

                    var precision = scores[i, j];
                    if (precision < 0.3)
                    {
                        continue;
                    }

                    var top = (int)(boxes[i, j, 0] * imageNow.Height);
                    var left = (int)(boxes[i, j, 1] * imageNow.Width);
                    var bottom = (int)(boxes[i, j, 2] * imageNow.Height);
                    var right = (int)(boxes[i, j, 3] * imageNow.Width);

                    var probability = Math.Round(precision, 2);
                    var box = new Rectangle(left, top, right - left, bottom - top);

                    String className = "";
                    if (typeNetwork == 1)
                    {
                        className = _nameGeneralClasess[classIndex];
                    }
                    if (typeNetwork == 2)
                    {
                        className = _nameTrashTypeClasess[classIndex];
                    }
                    if (typeNetwork == 3)
                    {
                        className = _nameMaskClasess[classIndex];
                    }

                    DetectedObjects.Add(new DetectedObject
                        {
                            Rectangle = box,
                            ClassId = classIndex,
                            ClassName = className,
                            Probability = probability
                        }
                        );
                }
            }

            return new DetectorMatch
            {
                DateTime = DateTime.Now,
                DetectedObjects = DetectedObjects,
                Frame = numberFrame
            };
        }

        public TFTensor CreateTensorFromImageFrame(ImageFrame imageFrame)
        {
            return TFTensor.FromBuffer(new TFShape(1, imageFrame.Height, imageFrame.Width, 3), imageFrame.Bytes, 0, imageFrame.Bytes.Length);
        }
    }
}
