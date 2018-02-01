using System.Drawing;
using System.Threading.Tasks;
using Mosaic.Bots;
using Mosaic.Layers;

namespace Mosaic.Creators {
    internal class RenderCreator : ICreator {
        private readonly ISize _size;
        private readonly Color[,] _pixels;

        public RenderCreator(ISize size) {
            _size = size;
            _pixels = new Color[size.Width, size.Height];
        }

        public async Task Set(BotResult botResult) => await Task.Factory.StartNew(() => {
            for (var x = 0; x < botResult.Width; x++) {
                for (var y = 0; y < botResult.Height; y++) {
                    _pixels[botResult.Left + x, botResult.Top + y] = botResult.Colors[x, y];
                }
            }
        });

        public async Task Flush(string filename) => await Task.Factory.StartNew(() => {
            Broadcast.Start(this, $"Saving {filename}...");
            using (var bmp = new Bitmap(_size.Width, _size.Height)) {
                for (var x = 0; x < _size.Width; x++) {
                    for (var y = 0; y < _size.Height; y++) {
                        bmp.SetPixel(x, y, _pixels[x, y]);
                    }
                }
                bmp.Save(filename);
            }
            Broadcast.End(this);
        });
    }
}