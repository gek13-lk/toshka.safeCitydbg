using System.Collections.Generic;

namespace Toshka.dbgSave.NeuralNetwork.Events
{
    public class EmissionEvent : DomainObject
    {
        public override long Id { get; set; }

        public int? NumberArea { get; set; }

        public int FrameFrom { get; set; }
        public int FrameTo { get; set; }

        public int CountLife { get; set; }

        public int ClassId { get; set; }

        public string ClassName { get; set; }

        public double? Probability { get; set; }

        public double? Density { get; set; }

        public string PathVideo { get; set; }

        public  string TypeAlgorithm { get; set; }

        public List<DetectedObject> DetectedObjects;
    }
}
