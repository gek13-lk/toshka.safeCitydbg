using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Toshka.dbgSave.JsonModel
{
    public class ForecastModel
    {
        public DateTime Date { get; set; }
        public float DayBefore { get; set; }
        public float Fullness { get; set; }
    }
}
