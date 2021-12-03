using System;
using Toshka.dbgSave.NeuralNetwork.Events;

namespace Toshka.dbgSave.NeuralNetwork.DetectorTools.Tracker
{
    public abstract class AbstractObjectBinder
    {
        protected abstract EmissionOnline TryBindToEmission(DetectedObject detectedObject, long screenLayoutId,
            DateTime detectionTime);

        public virtual EmissionOnline GetBindedEmission(DetectedObject detectedObject, long screenLayoutId,
            DateTime detectionTime)
        {
            var emission = TryBindToEmission(detectedObject, screenLayoutId, detectionTime);
            if (emission != null)
            {
                AddBindedObject(detectedObject, emission, screenLayoutId);
            }

            return emission;
        }

        protected abstract void AddBindedObject(DetectedObject detectedObject, EmissionOnline emission,
            long screenLayoutId);
    }
}