using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Toshka.dbgSave.Model
{
    [Keyless]
    public class ModelOutput 
    {
        public float[] ForecastedRentals { get; set; }

        public float[] LowerBoundRentals { get; set; }

        public float[] UpperBoundRentals { get; set; }
    }
}
