using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Toshka.dbgSave.Helpers
{
    public static class BitmapExtensions
    {
        public static MemoryStream ToStream(this Bitmap image, ImageFormat format)
        {
            var stream = new System.IO.MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }
    }
}