using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using Mosaic.Bots;

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

        public async Task Set(BotResult botResult) => await Task.Factory.StartNew(() => {
            for (var x = 0; x < botResult.Width; x++) {
                for (var y = 0; y < botResult.Height; y++) {
                    for (var step = 1d; step >= 0; step -= 0.5d) {
                        if (step > botResult.Odds[x, y]) {
                            continue;
                        }

                        _steps[step].SetPixel(botResult.Left + x, botResult.Top + y, botResult.Colors[x, y]);
                    }
                }
            }
        });

        public async Task Flush(string filename) => await Task.Factory.StartNew(() => {
            filename = $"{filename}-animated.gif";
            Broadcast.Start(this, $"Saving {filename}...");

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

            Broadcast.End(this);
        });

        public void Dispose() {
            foreach (var step in _steps) {
                step.Value.Dispose();
            }
        }
    }
}