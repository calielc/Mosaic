using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using Mosaic.Imaging;

namespace Mosaic.Savers {
    internal sealed class TileSaver : ISaver, IDisposable {
        private readonly ISize _size;
        private readonly string _filename;
        private readonly Broadcast _broadcast;
        private readonly ConcurrentDictionary<int, Bitmap> _frames = new ConcurrentDictionary<int, Bitmap>();

        public TileSaver(ISize size, string filename, Broadcast broadcast) {
            _size = size;
            _filename = AdjustFilename();
            _broadcast = broadcast;

            _frames[-1] = new Bitmap(size.Width, size.Height);

            string AdjustFilename() {
                var directory = Path.GetDirectoryName(filename);
                var name = Path.GetFileNameWithoutExtension(filename);

                return Path.Combine(directory, $"{name}.gif");
            }
        }

        public async Task Set(ILayerResult input) => await Task.Run(() => {
            if (_frames.TryGetValue(input.Order, out var image) == false) {
                _frames[input.Order] = image = NewBitmap(input.Order);
            }

            for (var x = 0; x < input.Width; x++) {
                for (var y = 0; y < input.Height; y++) {
                    image.SetPixel(input.Left + x, input.Top + y, input.Colors[x, y]);
                }
            }
        });

        private Bitmap NewBitmap(int inputOrder) {
            if (_frames.TryGetValue(inputOrder - 1, out var previous)) {
                return (Bitmap)previous.Clone();
            }
            return new Bitmap(_size.Width, _size.Height);
        }

        public async Task Run() => await Task.Run(() => {
            _broadcast.Start(this, $"Saving {_filename}...");
            try {
                var magickImages = _frames
                    .OrderBy(pair => pair.Key)
                    .Select(pair => new MagickImage(pair.Value) {
                        AnimationDelay = 75
                    });

                using (var magickImageCollection = new MagickImageCollection(magickImages)) {
                    magickImageCollection.Last().AnimationDelay = 3000;

                    magickImageCollection.Quantize(new QuantizeSettings {
                        Colors = int.MaxValue,
                        ColorSpace = ColorSpace.RGB
                    });

                    magickImageCollection.Write(_filename);
                }
            }
            finally {
                _broadcast.End(this);
            }
        });

        public void Dispose() {
            foreach (var image in _frames.Values) {
                image.Dispose();
            }
        }
    }
}