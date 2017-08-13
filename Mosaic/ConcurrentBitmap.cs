using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Mosaic
{
    internal sealed class ConcurrentBitmap
    {
        private Color[,] _pixels;

        public ConcurrentBitmap(string filename)
        {
            using (var bmp = new Bitmap(filename))
            {
                Width = bmp.Width;
                Height = bmp.Height;

                // Lock the bitmap's bits.  
                var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                var stride = bmpData.Stride;
                var bytes = Math.Abs(stride) * bmp.Height;
                var rgbValues = new byte[bytes];

                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);

                _pixels = new Color[Width, Height];
                Parallel.For(0, Width, x =>
                {
                    for (int y = 0; y < Height; y++)
                    {
                        var pos = (y * stride) + (x * 3);
                        var b = rgbValues[pos];
                        var g = rgbValues[pos + 1];
                        var r = rgbValues[pos + 2];

                        _pixels[x, y] = Color.FromArgb(255, r, g, b);
                    }
                });
            }
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Color GetPixel(int x, int y) => _pixels[x, y];
    }
}