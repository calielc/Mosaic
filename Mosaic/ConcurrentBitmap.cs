using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Mosaic {
    internal sealed class ConcurrentBitmap {
        private readonly SingleColor[,] _pixels;

        public ConcurrentBitmap(string filename) {
            using (var bmp = new Bitmap(filename)) {
                var width = bmp.Width;
                var height = bmp.Height;

                // Lock the bitmap's bits.  
                var rect = new Rectangle(0, 0, width, height);
                var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                // Get the address of the first line.
                var ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                var stride = bmpData.Stride;
                var bytes = Math.Abs(stride) * height;
                var rgbValues = new byte[bytes];

                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);

                _pixels = new SingleColor[width, height];
                Parallel.For(0, height, y => {
                    var strideSpan = y * stride;
                    for (var x = 0; x < width; x++) {
                        var pos = strideSpan + x * 3;
                        _pixels[x, y] = new SingleColor(rgbValues[pos + 2], rgbValues[pos + 1], rgbValues[pos]);
                    }
                });
            }
        }

        public int Width => _pixels.GetLength(0);

        public int Height => _pixels.GetLength(1);

        public SingleColor this[int x, int y] => _pixels[x, y];
    }
}