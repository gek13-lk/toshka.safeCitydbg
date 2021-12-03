using System;

namespace Toshka.dbgSave.Model
{
    public class Event : BaseEntity
    {
        public long CameraId { get; set; }

        public DateTime StartTime { get; set; }
        public string MainType { get; set; }
        public DateTime EndTime { get; set; }

        public string Distance { get; set; }

        public string Subtype { get; set; }
    }
}
