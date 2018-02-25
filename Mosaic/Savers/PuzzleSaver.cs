using System.Drawing;
using System.Threading.Tasks;
using Mosaic.Imaging;

namespace Mosaic.Savers {
    internal sealed class PuzzleSaver : ISaver {
        private readonly ISize _size;
        private readonly Broadcast _broadcast;
        private readonly Color[,] _pixels;
        private readonly string _filename;

        public PuzzleSaver(ISize size, string filename, Broadcast broadcast) {
            _size = size;
            _filename = filename;
            _broadcast = broadcast;
            _pixels = new Color[size.Width, size.Height];
        }

        public async Task Set(ILayerResult input) => await Task.Factory.StartNew(() => {
            Parallel.For(0, input.Width, x => {
                for (var y = 0; y < input.Height; y++) {
                    _pixels[input.Left + x, input.Top + y] = input.Colors[x, y];
                }
            });
        });

        public async Task Run() => await Task.Factory.StartNew(() => {
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