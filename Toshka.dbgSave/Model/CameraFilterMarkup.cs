using System.Drawing;

namespace Toshka.dbgSave.Model
{
    //filter places on image
    public class CameraFilterMarkup : BaseEntity
    {
        public long CameraId { get; set; }
        public int dlx { get; set; }
        public int ulx { get; set; }
        public int dly { get; set; }
        public int uly { get; set; }
        public int drx { get; set; }
        public int dry { get; set; }
        public int ury { get; set; }
        public int urx { get; set; }
    }
}