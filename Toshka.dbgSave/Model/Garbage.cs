using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Toshka.dbgSave.Model
{
    public class Garbage : BaseEntity
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Type { get; set; } //Заполненность red/yellow/green
        public string Fullness { get; set; }//fullness from predict
        public DateTime DateExport { get; set; }
        public bool IsCamera { get; set; } //Есть камера или нет
        public string Video { get; set; }
    }
}
