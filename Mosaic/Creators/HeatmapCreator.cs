using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Mosaic.Layers;

namespace Mosaic.Creators {
    internal sealed class HeatmapCreator : ICreator {
        private readonly ISize _size;
        private readonly Color[,] _pixels;

        public HeatmapCreator(ISize size, Broadcast broadcast) {
            _size = size;
            _pixels = new Color[size.Width, size.Height];
        }

        public Broadcast Broadcast { get; set; }

        public async Task Set(ILayerResult input) => await Task.Run(() => {
            Parallel.For(0, input.Width, x => {
                for (var y = 0; y < input.Height; y++) {
                    var color = Interpolate(input.Odds[x, y]);

                    _pixels[input.Left + x, input.Top + y] = color;
                }
            });
        });

        private static Color Interpolate(double percent) {
            var source = Color.Black;
            var target = Color.White;

            var r = (byte)(source.R + (target.R - source.R) * percent);
            var g = (byte)(source.G + (target.G - source.G) * percent);
            var b = (byte)(source.B + (target.B - source.B) * percent);

            return Color.FromArgb(r, g, b);
        }

        public async Task Flush(string filename) => await Task.Run(() => {
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
                filename = Path.Combine(directory, $"{name}-heat.jpg");
            }
        });
    }
}