using System;
using System.Collections.Generic;

namespace Toshka.dbgSave.NeuralNetwork.Events
{
    public class DetectorMatch : DomainObject
    {
        public override long Id { get; set; }

        public int Frame { get; set; }

        public DateTime DateTime { get; set; }
        public List<DetectedObject> DetectedObjects { get; set; }
    }
}