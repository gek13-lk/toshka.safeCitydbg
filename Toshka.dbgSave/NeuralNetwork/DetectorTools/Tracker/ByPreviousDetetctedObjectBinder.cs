using System.Collections.Generic;
using System.Linq;
using Toshka.dbgSave.NeuralNetwork.Events;

namespace Toshka.dbgSave.NeuralNetwork.DetectorTools.Tracker
{
    public static class ByPreviousDetectedObjectBinder
    {
        public static EmissionEvent GetBindedEmission(DetectedObject detectedObject,
            List<EmissionEvent> emissionEvents, List<Shape> shapes)
        {
            var emission = TryBindToEmission(detectedObject, emissionEvents, shapes);
            emission?.DetectedObjects.Add(detectedObject);

            return emission;
        }

        private static EmissionEvent TryBindToEmission(DetectedObject detectedObject,
            List<EmissionEvent> emissionEvents, List<Shape> shapes)
        {
            double _verticalFault = 0.2;
            int _minPrevObjShapeIntersect = 60;
            int _minPrevObjIntersect = 50;

            List<int> suitableClassIds = new List<int>();
            if (detectedObject.ClassId == 1 || detectedObject.ClassId == 2 || detectedObject.ClassId == 3)
            {
                suitableClassIds.Add(1);
                suitableClassIds.Add(2);
                suitableClassIds.Add(3);
            }
            else
            {
                suitableClassIds.Add(detectedObject.ClassId);
            }

            EmissionEvent emission = null;
            var previousPercentage = 0d;

            var shapesByArea = shapes.FindAll(s => TrackerUtils.IsIntersectObjectAndShape(s, detectedObject));

            // Если детектируемый объект не пересекается с границами шейпа и нижняя граница объекта не находится над шейпом
            // то такое событие не должно к нему привязываться
            var emissionDetectedObjects = emissionEvents
                .Where(t =>
                {
                    return suitableClassIds.Contains(t.ClassId) &&
                           (t.NumberArea == null || shapesByArea.Exists(s =>
                                s.Number == t.NumberArea));
                }).ToList();

            //// Если объект более чем на 60% пересекается с другим шейпом, то относим его к нему
            var shapesMaxYInShape =
                shapesByArea.Where(s =>
                    TrackerUtils.MinYInShapePredicate(s, detectedObject, _verticalFault));
            var maxIntersectShape =
                TrackerUtils.GetShapeByMaxIntersect(shapesMaxYInShape, detectedObject,
                    _minPrevObjShapeIntersect);

            // Сначала пытаемся найти по области пересечения объектов, после по пересечению по вертикали
            foreach (var detectedObj in emissionDetectedObjects)
            {
                foreach (var previousObj in detectedObj.DetectedObjects.Where(d =>
                    TrackerUtils.IsIntersectDetectedObjects(d, detectedObject)))
                {
                    var percentage =
                        TrackerUtils.GetAvgIntersectPercentage(previousObj.Rectangle, detectedObject.Rectangle);
                    if (percentage < _minPrevObjIntersect || percentage < previousPercentage ||
                        maxIntersectShape != null &&
                        !TrackerUtils.IsShapeAsEmission(maxIntersectShape, detectedObj))
                        continue;
                    previousPercentage = percentage;
                    emission = detectedObj;
                }
            }


            if (emission != null || maxIntersectShape != null)
            {
                return emission != null ? null : emission;
            }

            foreach (var eventDo in emissionDetectedObjects)
            {
                foreach (var previousObj in eventDo.DetectedObjects.Where(d =>
                    TrackerUtils.IsIntersectDetectedObjects(d, detectedObject)))
                {
                    var percentage = TrackerUtils.GetVerticalIntersect(previousObj.Rectangle, detectedObject.Rectangle,
                        _verticalFault);
                    if (percentage < 20 || percentage < previousPercentage) continue;
                    previousPercentage = percentage;
                    emission = eventDo;
                }
            }

            return emission != null ? null : emission;
        }
    }
}