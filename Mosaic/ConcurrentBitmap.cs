using System;
using System.Drawing;

namespace Mosaic
{
    internal sealed class ConcurrentBitmap
    {
        private readonly int _stride;
        private readonly byte[] _rgbValues;

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
                var ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                _stride = bmpData.Stride;
                var bytes = Math.Abs(_stride) * bmp.Height;
                _rgbValues = new byte[bytes];

                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, _rgbValues, 0, bytes);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);
            }
        }

        public int Width { get; }

        public int Height { get; }

        public Color GetPixel(int x, int y)
        {
            var pos = y * _stride + x * 3;
            var b = _rgbValues[pos];
            var g = _rgbValues[pos + 1];
            var r = _rgbValues[pos + 2];

            return Color.FromArgb(255, r, g, b);
        }
    }
}