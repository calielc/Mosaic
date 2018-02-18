using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Mosaic.Layers;
using Mosaic.Queue;

namespace Mosaic.Jobs {
    internal sealed class HeatmapCreator : ICreator, IActivity {
        private readonly ISize _size;
        private readonly string _filename;
        private readonly Broadcast _broadcast;
        private readonly Color[,] _pixels;

        public HeatmapCreator(ISize size, string filename, Broadcast broadcast) {
            _size = size;
            _filename = AdjustFilename();
            _broadcast = broadcast;
            _pixels = new Color[size.Width, size.Height];

            string AdjustFilename() {
                var directory = Path.GetDirectoryName(filename);
                var name = Path.GetFileNameWithoutExtension(filename);
                return Path.Combine(directory, $"{name}-heat.jpg");
            }
        }

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

        public async Task Run() => await Task.Run(() => {
            _broadcast.Start(this, $"Saving {_filename}...");
            try {
                using (var bmp = new Bitmap(_size.Width, _size.Height)) {
                    for (var x = 0; x < _size.Width; x++) {
                        for (var y = 0; y < _size.Height; y++) {
                            bmp.SetPixel(x, y, _pixels[x, y]);
                        }
                    }

                    bmp.Save(_filename);
                }
            }
            finally {
                _broadcast.End(this);
            }
        });
    }
}