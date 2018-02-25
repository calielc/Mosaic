using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Mosaic.Imaging {
    [DebuggerDisplay("Name: {Name}, Width: {Width}: Height: {Height}")]
    internal sealed class Image : IEquatable<Image>, ISize {
        private readonly string _filename;
        private readonly RGBColor[,] _pixels;
        private readonly string _name;
        private readonly int _width;
        private readonly int _height;
        private RGBColor[,] _reduced;

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

            Raw = Reduced = new PixelIndexer(_pixels);
        }

        public string Name => _name;

        public int Width => _width;
        public int Height => _height;

        public PixelIndexer Raw { get; }
        public PixelIndexer Reduced { get; private set; }

        public RGBColor this[int x, int y] => _pixels[x, y];

        public void Reduce(IReadOnlyDictionary<RGBColor, RGBColor> palette) {
            _reduced = new RGBColor[_width, _height];

            using (var bmp = new Bitmap(_width, _height)) {
                for (var x = 0; x < _width; x++) {
                    for (var y = 0; y < _height; y++) {
                        var color = _pixels[x, y];
                        var reduced = _reduced[x, y] = palette[color];

                        bmp.SetPixel(x, y, reduced);
                    }
                }

                var directory = Path.GetDirectoryName(_filename);
                var filename = Path.GetFileName(_filename);

                directory = Path.Combine(directory, $"reduced-{palette.Count}");
                Directory.CreateDirectory(directory);

                bmp.Save(Path.Combine(directory, filename));
            }

            Reduced = new PixelIndexer(_reduced);
        }

        public bool Equals(Image other) {
            if (other is null) {
                return false;
            }

            return ReferenceEquals(this, other) || string.Equals(_filename, other._filename);
        }

        public override bool Equals(object obj) {
            if (obj is null) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is Image image && Equals(image);
        }

        public override int GetHashCode() {
            return _filename != null ? _filename.GetHashCode() : 0;
        }
    }
}