using System;
using System.Drawing;

namespace Toshka.dbgSave.NeuralNetwork.ProcessVideo
{
    public static class ColorModels
    {
        public static byte RgbToGrayscale(byte r, byte g, byte b)
        {
            return (byte)(r * 0.3 + g * 0.59 + b * 0.11);
        }

        static public void YcbCrtoRGB(int Y, int Cb, int Cr, out int r, out int g, out int b)
        {
            r = (int)(Y + 1.402 * (Cr - 128));
            g = (int)(Y - 0.34414 * (Cb - 128) - 0.71414 * (Cr - 128));
            b = (int)(Y + 1.772 * (Cb - 128));

            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));
        }

        //переход в пространство YCbCr
        static public void RGBtoYcbCr(Color color, out int Y, out int Cb, out int Cr)
        {
            int r = color.R;
            int g = color.G;
            int b = color.B;
            Y = Convert.ToInt32(0.299 * r + 0.587 * g + 0.114 * b);
            Cb = Convert.ToInt32(128 - 0.168736 * r - 0.331264 * g + 0.5 * b);
            Cr = Convert.ToInt32(128 + 0.5 * r - 0.418688 * g - 0.081312 * b);

            Y = Math.Max(0, Math.Min(255, Y));
            Cb = Math.Max(0, Math.Min(255, Cb));
            Cr = Math.Max(0, Math.Min(255, Cr));
        }

        static public void ToHSV(Color color, out double h, out double s, out double b)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            if (max == min)
                h = 0;
            else if (max == color.R && color.G >= color.B)
                h = 60 * (color.G - color.B) / (max - min) + 0;
            else if (max == color.R && color.G < color.B)
                h = 60 * (color.G - color.B) / (max - min) + 360;
            else if (max == color.G)
                h = 60 * (color.B - color.R) / (max - min) + 120;
            else
                h = 60 * (color.R - color.G) / (max - min) + 240;

            if (max == 0)
                s = 0;
            else
                s = 1d - 1d * min / max;

            b = max / 255d;
        }

        //Из HSV в RGB
        static public void ColorFromHSV(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {
                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        public static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
    }
}
