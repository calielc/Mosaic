using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;

namespace Mosaic.Creators {
    internal sealed class AnimatedGifCreator : ICreator, IDisposable {
        private readonly Dictionary<double, Bitmap> _steps;

        public AnimatedGifCreator(string filename) {
            using (var original = new Bitmap(filename)) {
                _steps = new Dictionary<double, Bitmap>();
                for (var step = 1d; step >= 0; step -= 0.5d) {
                    _steps[step] = new Bitmap(original);
                }
            }
        }

        public async Task Set(ILayerResult input) => await Task.Run(() => {
            Parallel.For(0, input.Width, x => {
                for (var y = 0; y < input.Height; y++) {
                    for (var step = 1d; step >= 0; step -= 0.5d) {
                        if (step > input.Odds[x, y]) {
                            continue;
                        }

                        _steps[step].SetPixel(input.Left + x, input.Top + y, input.Colors[x, y]);
                    }
                }
            });
        });

        public async Task Flush(string filename) => await Task.Run(() => {
            var directory = Path.GetDirectoryName(filename);
            var name = Path.GetFileNameWithoutExtension(filename);
            filename = Path.Combine(directory, $"{name}-animated.gif");

            Broadcast.Start(this, $"Saving {filename}...");
            try {
                var magickImages = _steps
                    .OrderByDescending(pair => pair.Key)
                    .Select(pair => new MagickImage(pair.Value) {
                        AnimationDelay = 25
                    });

                using (var magickImageCollection = new MagickImageCollection(magickImages)) {
                    magickImageCollection.First().AnimationDelay = 200;
                    magickImageCollection.Last().AnimationDelay = 300;

                    magickImageCollection.Quantize(new QuantizeSettings {
                        Colors = int.MaxValue,
                        ColorSpace = ColorSpace.RGB
                    });

                    magickImageCollection.Write(filename);
                }
            }
            finally {
                Broadcast.End(this);
            }
        });

        public void Dispose() {
            foreach (var step in _steps) {
                step.Value.Dispose();
            }
        }
    }
}