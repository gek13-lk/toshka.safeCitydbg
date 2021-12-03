using System.Collections.Generic;
using System.Drawing;

namespace Toshka.dbgSave.NeuralNetwork.Events
{
    public class Shape
    {
        public List<PointF> Points = new List<PointF>();

        public int Number { get; set; }

        public bool IsValid { get; set; }
    }
}
