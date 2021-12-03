using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Toshka.dbgSave.NeuralNetwork.ProcessVideo
{
    public class PointsConverter
    {
        private SizeF _originalSize;
        private SizeF _frameSize;

        private float _resizeFactor;
        private float _leftBorderPos;
        private float _topBorderPos;

        public RectangleF ImageRectangleByParrent { get; set; }


        public SizeF OriginalSize
        {
            get { return _originalSize; }
            set
            {
                if (value.Equals(_originalSize))
                {
                    return;
                }

                _originalSize = value;
                RecalcFactor();
            }
        }

        public SizeF FrameSize
        {
            get { return _frameSize; }
            set
            {
                if (value.Equals(_frameSize))
                {
                    return;
                }

                _frameSize = value;
                RecalcFactor();
            }
        }

        private void RecalcFactor()
        {
            if (_frameSize.IsEmpty || _originalSize.IsEmpty)
            {
                return;
            }

            var widthFactor = _originalSize.Width / _frameSize.Width;
            var heightFactor = _originalSize.Height / _frameSize.Height;
            _resizeFactor = Math.Max(widthFactor, heightFactor);
            var newImageSize = new SizeF(_originalSize.Width / _resizeFactor, _originalSize.Height / _resizeFactor);
            _leftBorderPos = _frameSize.Width / 2 - newImageSize.Width / 2;
            _topBorderPos = _frameSize.Height / 2 - newImageSize.Height / 2;
            ImageRectangleByParrent =
                new RectangleF(_leftBorderPos, _topBorderPos, newImageSize.Width, newImageSize.Height);
        }

        public IEnumerable<PointF> ConvertPointsToRelativeSize(List<PointF> points)
        {
            if (_frameSize.IsEmpty || _originalSize.IsEmpty)
            {
                return points;
            }

            return points.Select(p =>
                new PointF(_leftBorderPos + p.X / _resizeFactor, _topBorderPos + p.Y / _resizeFactor));
        }

        public Rectangle ConvertRectangleToRelativeSize(Rectangle originalRectangle)
        {
            if (_frameSize.IsEmpty || _originalSize.IsEmpty)
            {
                return originalRectangle;
            }

            return new Rectangle((int)(_leftBorderPos + originalRectangle.X / _resizeFactor),
                (int)(_topBorderPos + originalRectangle.Y / _resizeFactor),
                (int)(originalRectangle.Width / _resizeFactor), (int)(originalRectangle.Height / _resizeFactor));
        }

        public Point ConvertPointFromRelativeSize(Point point)
        {
            return ConvertPointFromRelativeSize(point.X, point.Y);
        }

        public Point ConvertPointFromRelativeSize(int x, int y)
        {
            if (_frameSize.IsEmpty || _originalSize.IsEmpty)
            {
                return new Point(x, y);
            }

            return new Point((int)((x - _leftBorderPos) * _resizeFactor), (int)((y - _topBorderPos) * _resizeFactor));
        }

        public float ConvertValueFromRelativeSize(float value)
        {
            if (_frameSize.IsEmpty || _originalSize.IsEmpty)
            {
                return value;
            }

            return value * _resizeFactor;
        }

        public Point ConvertPointToRelativeSize(int x, int y)
        {
            if (_frameSize.IsEmpty || _originalSize.IsEmpty)
            {
                return new Point(x, y);
            }

            return new Point((int)(_leftBorderPos + x / _resizeFactor), (int)(_topBorderPos + y / _resizeFactor));
        }

        public Point ConvertPointToRelativeSize(PointF point)
        {
            if (_frameSize.IsEmpty || _originalSize.IsEmpty)
            {
                return new Point((int)point.X, (int)point.Y);
            }

            return new Point((int)(_leftBorderPos + (int)point.X / _resizeFactor), (int)(_topBorderPos + (int)point.Y / _resizeFactor));
        }

        public bool FigureContains(List<PointF> shapePoints)
        {
            var sortedPointsX = shapePoints.OrderBy(p => p.X);
            var sortedPointsY = shapePoints.OrderBy(p => p.Y);

            if (PointContains(ConvertPointToRelativeSize(sortedPointsX.First())) &&
                PointContains(ConvertPointToRelativeSize(sortedPointsX.Last())) &&
                PointContains(ConvertPointToRelativeSize(sortedPointsY.First())) &&
                PointContains(ConvertPointToRelativeSize(sortedPointsY.Last())))
            {
                return true;
            }

            return false;
        }

        public Point ConvertToContains(Point point)
        {
            if (point.IsEmpty)
            {
                return point;
            }

            if (point.X < ImageRectangleByParrent.X)
            {
                point.X = (int)ImageRectangleByParrent.X;
            }
            else if (point.X > ImageRectangleByParrent.Right)
            {
                point.X = (int)ImageRectangleByParrent.Right;
            }

            if (point.Y < ImageRectangleByParrent.Y)
            {
                point.Y = (int)ImageRectangleByParrent.Y;
            }
            else if (point.Y > ImageRectangleByParrent.Bottom)
            {
                point.Y = (int)ImageRectangleByParrent.Bottom;
            }

            return point;
        }

        public bool PointContains(int x, int y)
        {
            if (_frameSize.IsEmpty || _originalSize.IsEmpty)
            {
                return false;
            }

            return ImageRectangleByParrent.Contains(x, y);
        }

        public bool PointContains(Point point)
        {
            if (_frameSize.IsEmpty || _originalSize.IsEmpty)
            {
                return false;
            }

            return ImageRectangleByParrent.Contains(point.X, point.Y);
        }

        public bool PointRelativeContains(int x, int y)
        {
            if (_frameSize.IsEmpty || _originalSize.IsEmpty)
            {
                return false;
            }

            return ImageRectangleByParrent.Contains((int)(_leftBorderPos + x / _resizeFactor),
                (int)(_topBorderPos + y / _resizeFactor));
        }
    }
}