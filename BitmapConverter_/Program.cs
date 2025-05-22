using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BitmapConverter_
{
    internal class Program
    {
        private static string OriginalPath = "C:\\Users\\tawle\\Desktop\\FORMAT.png";
        private static string DestinationPath = "C:\\Users\\tawle\\Desktop\\FORMAT_converted.png";

        static void Main(string[] args)
        {
            Bitmap bitmap = new Bitmap(OriginalPath);

            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];

            Marshal.Copy(ptr, rgbValues, 0, bytes);

            int width = bitmap.Width;
            int height = bitmap.Height;

            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4;

                    byte a = rgbValues[index + 3];
                    byte r = rgbValues[index + 2];
                    byte g = rgbValues[index + 1];
                    byte b = rgbValues[index];

                    Color c = ConvertToBgr565(a, r, g, b);

                    rgbValues[index + 3] = c.A;
                    rgbValues[index + 2] = c.R;
                    rgbValues[index + 1] = c.G;
                    rgbValues[index] = c.B;
                }
            });

            Marshal.Copy(rgbValues, 0, ptr, bytes);

            bitmap.UnlockBits(bmpData);
            bitmap.Save(DestinationPath);
            bitmap.Dispose();


            Console.WriteLine("Bitmap modified and saved successfully.");
            Console.ReadLine();
        }

        private static Color ConvertToBgr565(byte a, byte r, byte g, byte b)
        {
            int r565 = (r * 31 + 127) / 255;
            int g565 = (g * 63 + 127) / 255;
            int b565 = (b * 31 + 127) / 255;

            r = (byte)((r565 * 255 + 15) / 31);
            g = (byte)((g565 * 255 + 31) / 63);
            b = (byte)((b565 * 255 + 15) / 31);

            return Color.FromArgb(a, r, g, b);
        }

        private static byte Trim(byte value, int n)
        {
            byte mask = (byte)~(byte.MaxValue << n);
            byte output = (byte)(value & ~mask);

            int r = value & mask;
            int d = 1 << (n);

            if (r > d / 2) output = (byte)(output + d);

            return output;
        }
    }
}