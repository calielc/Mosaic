using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Mosaic.Layers;

namespace Mosaic.Creators {
    internal sealed class TilesCreator : ICreator {
        private readonly ISize _size;
        private readonly Color[,] _pixels;

        public TilesCreator(ISize size) {
            _size = size;
            _pixels = new Color[size.Width, size.Height];
        }

        public async Task Set(ILayerResult input) => await Task.Run(() => {
            var color = GetColor();

            Parallel.For(0, input.Width, x => {
                for (var y = 0; y < input.Height; y++) {
                    _pixels[input.Left + x, input.Top + y] = color;
                }
            });

            Color GetColor() {
                var col = Convert.ToInt32(Math.Round(1d * input.Left / input.Width)) % 2;
                var row = Convert.ToInt32(Math.Round(1d * input.Top / input.Height)) % 2;

                if (col == 0) {
                    return row == 0
                        ? Color.FromArgb(170, 255, 130)
                        : Color.FromArgb(215, 130, 255);
                }

                return row == 0
                    ? Color.FromArgb(255, 130, 170)
                    : Color.FromArgb(130, 255, 215);
            }
        });

        public async Task Flush(string filename) => await Task.Factory.StartNew(() => {
            AdjustFilename();

            Broadcast.Start(this, $"Saving {filename}...");
            try {
                using (var bmp = new Bitmap(_size.Width, _size.Height)) {
                    for (var x = 0; x < _size.Width; x++) {
                        for (var y = 0; y < _size.Height; y++) {
                            bmp.SetPixel(x, y, _pixels[x, y]);
                        }
                    }
                    bmp.Save(filename);
                }
            }
            finally {
                Broadcast.End(this);
            }

            void AdjustFilename() {
                var directory = Path.GetDirectoryName(filename);
                var name = Path.GetFileNameWithoutExtension(filename);

                filename = Path.Combine(directory, $"{name}-layers.jpg");
            }
        });
    }
}