using System.Drawing;

namespace Toshka.dbgSave.NeuralNetwork.Events
{
    public class DetectedObject
    {
        public Rectangle Rectangle { get; set; }

        public int ClassId { get; set; }

        public int NetworkId { get; set; }
        public string ClassName { get; set; }

        public double Probability { get; set; }

        public double Density { get; set; }
    }
}