using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Mosaic.Layers {
    internal sealed class Image : ILayer {
        private readonly string _filename;
        private readonly RGBColor[,] _pixels;
        private RGBColor[,] _reduced;
        private readonly string _name;
        private readonly int _width;
        private readonly int _height;

        public Image(string filename) {
            _filename = filename;
            _name = Path.GetFileNameWithoutExtension(filename);

            using (var bmp = new Bitmap(filename)) {
                _width = bmp.Width;
                _height = bmp.Height;

                // Lock the bitmap's bits.  
                var rect = new Rectangle(0, 0, _width, _height);
                var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                // Get the address of the first line.
                var ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                var stride = bmpData.Stride;
                var bytes = Math.Abs(stride) * _height;
                var rgbValues = new byte[bytes];

                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);

                _pixels = _reduced = new RGBColor[_width, _height];
                Parallel.For(0, _height, y => {
                    var strideSpan = y * stride;
                    for (var x = 0; x < _width; x++) {
                        var pos = strideSpan + x * 3;
                        _pixels[x, y] = new RGBColor(rgbValues[pos + 2], rgbValues[pos + 1], rgbValues[pos]);
                    }
                });
            }
        }

        public string Name => _name;
        public int Width => _width;
        public int Height => _height;

        public RGBColor this[int x, int y] => _pixels[x, y];

        int IRectangle.Left => 0;
        int IRectangle.Top => 0;

        public void Reduce(IReadOnlyDictionary<RGBColor, RGBColor> palette) {
            _reduced = new RGBColor[_width, _height];
            using (var bmp = new Bitmap(_width, _height)) {
                for (var x = 0; x < _width; x++) {
                    for (var y = 0; y < _height; y++) {
                        var color = _pixels[x, y];
                        var reduced = _reduced[x, y] = palette[color];

                        bmp.SetPixel(x, y, reduced);
                    }
                };

                var directory = Path.GetDirectoryName(_filename);
                var filename = Path.GetFileName(_filename);

                directory = Path.Combine(directory, "reduced");
                Directory.CreateDirectory(directory);

                bmp.Save(Path.Combine(directory, filename));
            }
        }
    }
}