using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Toshka.dbgSave.Model;
using Toshka.dbgSave.NeuralNetwork.Events;
using Range = Emgu.CV.Structure.Range;

namespace Toshka.dbgSave.NeuralNetwork.DetectorTools.Tracker
{
    public static class TrackerUtils
    {
        public static bool IsIntersectObjectAndShape(Shape shape, DetectedObject detectedObject)
        {
            var minX = shape.Points.Min(p => p.X);
            var maxX = shape.Points.Max(p => p.X);
            var minY = shape.Points.Min(p => p.Y);
            var maxY = shape.Points.Max(p => p.Y);

            return !(detectedObject.Rectangle.Right <= minX) && !(detectedObject.Rectangle.Left >= maxX) &&
                   !(detectedObject.Rectangle.Bottom <= minY) && !(detectedObject.Rectangle.Top >= maxY);
        }

        public static Shape GetShapeByMaxIntersect(IEnumerable<Shape> shapes, DetectedObject detectedObject,
            double minMatchPercentage)
        {
            Shape shape = null;
            var maxIntersectPercent = 0d;

            foreach (var layoutShape in shapes)
            {
                var intersectPercent = GetPercentageIntersect(layoutShape, detectedObject);
                if (maxIntersectPercent > intersectPercent || intersectPercent < minMatchPercentage) continue;

                maxIntersectPercent = intersectPercent;
                shape = layoutShape;
            }

            return shape;
        }

        public static bool IsIntersectDetectedObjects(DetectedObject firstDetectedObj,
            DetectedObject secondDetectedObj)
        {
            return firstDetectedObj.Rectangle.Right > secondDetectedObj.Rectangle.Left &&
                   firstDetectedObj.Rectangle.Left < secondDetectedObj.Rectangle.Right &&
                   firstDetectedObj.Rectangle.Bottom > secondDetectedObj.Rectangle.Top &&
                   firstDetectedObj.Rectangle.Top < secondDetectedObj.Rectangle.Bottom;
        }

        //TODO:
        //public static Range CalculateDistance(DetectedObject detectedObject)
        //{
            
        //}

        public static Shape FindMatchedShape(DetectedObject detectedObject, List<Shape> shapes,
            double verticalFault)
        {
            if (shapes == null)
            {
                return null;
            }

            var screenLayoutShapes = shapes.FindAll(s => IsIntersectObjectAndShape(s, detectedObject));

            var shapesMaxYInShape =
                screenLayoutShapes.Where(s =>
                    MinYInShapePredicate(s, detectedObject, verticalFault));
            var shape = GetShapeByMaxIntersect(shapesMaxYInShape, detectedObject, 10d);

            if (shape != null)
            {
                return shape;
            }

            shape = GetShapeByMaxIntersect(screenLayoutShapes, detectedObject, 0);

            return shape;
        }

        public static double GetIntersectPercentage(Rectangle firstRect, Rectangle secondRect)
        {
            var intersectRec = Rectangle.Intersect(firstRect, secondRect);
            var minRectSquare = Math.Min(firstRect.Width * firstRect.Height, secondRect.Width * secondRect.Height);
            var percentage = intersectRec.Width * intersectRec.Height * 100f / minRectSquare;
            return percentage;
        }

        public static bool MinYInShapePredicate(Shape shape, DetectedObject detectedObject, double heightCoef)
        {
            // Добавочный коэфициент на случай, если объект найден немного выше разметки
            var minY = shape.Points.Min(p => p.Y);
            var maxY = shape.Points.Max(p => p.Y);
            var heightFault = (maxY - minY) * heightCoef;

            return detectedObject.Rectangle.Bottom <= maxY + heightFault;
        }

        public static double GetPercentageIntersect(Shape shape, DetectedObject detectedObject)
        {
            var area = 0d;

            var markedPath = new GraphicsPath();
            markedPath.AddLines(shape.Points.ToArray());
            var markedRegion = new Region(markedPath);
            if (markedRegion.IsVisible(detectedObject.Rectangle))
            {
                markedRegion.Intersect(detectedObject.Rectangle);
                area = GetRegionArea(markedRegion);
            }

            markedRegion.Dispose();
            markedPath.Dispose();

            return area * 100 / CalcShapeArea(detectedObject.Rectangle);
        }

        public static double CalcShapeArea(Rectangle shape)
        {
            return shape.Height * shape.Width;
        }

        public static double GetRegionArea(Region region)
        {
            return region.GetRegionScans(new Matrix())
                .Sum(regionScan => regionScan.Height * regionScan.Width);
        }

        public static double GetAvgIntersectPercentage(Rectangle firstRect, Rectangle secondRect)
        {
            var intersectRec = Rectangle.Intersect(firstRect, secondRect);
            var percentage = intersectRec.Width * intersectRec.Height * 100f * 2 /
                             (firstRect.Width * firstRect.Height + secondRect.Width * secondRect.Height);
            return percentage;
        }

        public static bool IsShapeAsEmission(Shape shape, EmissionEvent emission)
        {
            return shape.Number == emission.NumberArea;
        }

        public static double GetVerticalIntersect(Rectangle firstBox, Rectangle secondBox, double heightFault)
        {
            if (firstBox.Left > secondBox.Right || firstBox.Right < secondBox.Left)
            {
                return 0;
            }

            var boxes = new[] { firstBox, secondBox };
            boxes = boxes.OrderBy(b => b.Bottom).ToArray();
            var upBox = new Rectangle(boxes[0].X, boxes[0].Y, boxes[0].Width,
                (int)(boxes[0].Height * (1 + heightFault)));
            var bottomBox = new Rectangle((int)(boxes[1].X - boxes[1].Height * heightFault), boxes[1].Y,
                boxes[1].Width,
                boxes[1].Height);
            return GetIntersectPercentage(upBox, bottomBox);
        }

        public static List<DetectedObject> MergeDetectedObjects(List<DetectedObject> detectedObjects, int minIntersectPercentage)
        {
            if (!detectedObjects.Any())
            {
                return detectedObjects;
            }

            var detectedObjectsDict = new Dictionary<long, List<DetectedObject>>();

            /*foreach (var groupObj in detectedObjects)
            {
                foreach (var detectedObject in groupObj)
                {
                    if (!detectedObjectsDict.ContainsKey(groupObj.Key))
                    {
                        detectedObjectsDict.Add(groupObj.Key, new List<DetectedObject>
                        {
                            detectedObject
                        });
                    }
                    else
                    {
                        var mergedDetectedObjects = new List<DetectedObject>();
                        var toMergeObjects = new List<DetectedObject>
                        {
                            detectedObject
                        };
                        foreach (var previousDetectedObject in detectedObjectsDict[groupObj.Key])
                        {
                            if (TrackerUtils.GetIntersectPercentage(previousDetectedObject.Rectangle,
                                    detectedObject.Rectangle) < minIntersectPercentage)
                            {
                                mergedDetectedObjects.Add(previousDetectedObject);
                            }
                            else
                            {
                                toMergeObjects.Add(previousDetectedObject);
                            }
                        }

                        mergedDetectedObjects.Add(MergeDetectedObjects(toMergeObjects));

                        detectedObjectsDict[groupObj.Key] = mergedDetectedObjects;
                    }
                }
            }*/

            return detectedObjectsDict.Values
                .SelectMany(x => x)
                .ToList();
        }

        public static DetectedObject MergeDetectedObjects(List<DetectedObject> detectedObjects)
        {
            if (detectedObjects.Count == 1)
            {
                return detectedObjects.First();
            }

            var minX = detectedObjects.Min(ddo => ddo.Rectangle.X);
            var minY = detectedObjects.Min(ddo => ddo.Rectangle.Y);
            var maxX = detectedObjects.Max(ddo => ddo.Rectangle.Right);
            var maxY = detectedObjects.Max(ddo => ddo.Rectangle.Bottom);

            return new DetectedObject
            {
                ClassId = detectedObjects.Max(d => d.ClassId),
                Density = detectedObjects.Max(d => d.Density),
                Probability = detectedObjects.Max(d => d.Probability),
                ClassName = detectedObjects.First().ClassName,
                Rectangle = new Rectangle(minX, minY, maxX - minX, maxY - minY)
            };
        }

    }
}