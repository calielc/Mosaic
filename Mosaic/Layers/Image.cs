using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Mosaic.Layers {
    internal sealed class Image : ILayer {
        private readonly RGBColor[,] _pixels;

        public Image(string filename) {
            Name = Path.GetFileNameWithoutExtension(filename);

            using (var bmp = new Bitmap(filename)) {
                Width = bmp.Width;
                Height = bmp.Height;

                // Lock the bitmap's bits.  
                var rect = new Rectangle(0, 0, Width, Height);
                var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                // Get the address of the first line.
                var ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                var stride = bmpData.Stride;
                var bytes = Math.Abs(stride) * Height;
                var rgbValues = new byte[bytes];

                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);

                _pixels = new RGBColor[Width, Height];
                Parallel.For(0, Height, y => {
                    var strideSpan = y * stride;
                    for (var x = 0; x < Width; x++) {
                        var pos = strideSpan + x * 3;
                        _pixels[x, y] = new RGBColor(rgbValues[pos + 2], rgbValues[pos + 1], rgbValues[pos]);
                    }
                });
            }
        }

        public string Name { get; }

        public int Width { get; }
        public int Height { get; }

        public RGBColor this[int x, int y] => _pixels[x, y];

        int IRectangle.Left => 0;
        int IRectangle.Top => 0;
    }
}