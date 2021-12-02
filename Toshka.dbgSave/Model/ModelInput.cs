using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Toshka.dbgSave.Model
{
    public class ModelInput : BaseEntity
    {
        public DateTime RentalDate { get; set; }

        public float Weekend { get; set; }

        public float Fullness { get; set; }

        public float Export { get; set; }
    }
}
