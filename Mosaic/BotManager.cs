using ImageMagick;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mosaic {
    public sealed class BotManager {
        private ConcurrentBitmapCollection _images;
        private PixelBot[] _bots;
        private double _botsCount;
        private string _baseFile;

        public string SearchDirectory { get; set; }
        public string SearchPattern { get; set; }

        public string DestinyDirectory { get; set; }
        public string DestinyFilename { get; set; }

        public bool UseParallel { get; set; }
        public bool Heatmap { get; set; }
        public bool AnimatedGif { get; set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public event EventHandler<string> OnText;
        public event EventHandler<double> OnProgress;

        public void LoadImages() {
            SendText("Loading images...");

            var filenames = Directory.GetFiles(SearchDirectory, SearchPattern);
            _images = new ConcurrentBitmapCollection(filenames);

            Width = _images.Width;
            Height = _images.Height;

            SendText($"Loaded {_images.Count:#,##0} images.");
            SendText($"Dimensions: {Width} x {Height}.");

            _baseFile = filenames.First();
        }

        public void LoadBots() {
            _botsCount = Width * Height;
            SendText($"Loading {_botsCount:#,##0} bots...");

            var index = 0;
            _bots = new PixelBot[Width * Height];
            Parallel.For(0, _bots.Length, coord => {
                var x = coord % Width;
                var y = coord / Width;

                var bot = new PixelBot(x, y);
                bot.Load(_images.Count, _images.GetPixel(x, y));

                _bots[coord] = bot;

                index++;
                if (index % 750 == 0) {
                    SendProgress(index / _botsCount);
                }
            });

            SendText("Done");
        }

        public async Task Save() {
            await SaveFinal();
            await SaveHeatmap();
            await SaveAnimatedGif();
        }

        private void SendText(string text) => OnText?.Invoke(this, text);

        private void SendProgress(double percentual) => OnProgress?.Invoke(this, percentual);

        private async Task SaveFinal() => await Task.Factory.StartNew(() => {
            SendText("Creating final...");
            using (var bmp = new Bitmap(_baseFile)) {
                var index = 0;
                foreach (var bot in _bots) {
                    bot.Save(bmp);

                    index++;
                    if (index % 750 == 0) {
                        SendProgress(index / _botsCount);
                    }
                }
                bmp.Save(Path.Combine(DestinyDirectory, DestinyFilename));

                SendProgress(index / _botsCount);
            }
            SendText("Done");
        });

        private async Task SaveAnimatedGif() => await Task.Factory.StartNew(() => {
            if (AnimatedGif == false) {
                return;
            }

            SendText("Creating Animated Gif...");
            using (var bmp = new Bitmap(_baseFile)) {
                using (var collection = new MagickImageCollection()) {
                    collection.Add(new MagickImage(bmp));

                    var step = 1d;
                    var orderedBots = _bots.OrderByDescending(bot => bot.Chance);
                    foreach (var bot in orderedBots) {
                        if (bot.Chance < step) {
                            collection.Add(new MagickImage(bmp));

                            SendProgress(1d - step);
                            step -= 0.05d;
                        }

                        bot.Save(bmp);
                    }

                    collection.Add(new MagickImage(bmp));

                    foreach (var item in collection) {
                        item.AnimationDelay = 25;
                    }
                    collection.First().AnimationDelay = 200;
                    collection.Last().AnimationDelay = 300;

                    collection.Quantize(new QuantizeSettings {
                        Colors = int.MaxValue,
                        ColorSpace = ColorSpace.RGB
                    });

                    var filename = Path.Combine(DestinyDirectory, $"{DestinyFilename}-animated.gif");
                    collection.Write(filename);
                }
            }
            SendText("Done");
        });

        private async Task SaveHeatmap() => await Task.Factory.StartNew(() => {
            if (Heatmap == false) {
                return;
            }

            SendText("Creating Heatmap...");
            using (var bmp = new Bitmap(_baseFile)) {
                foreach (var bot in _bots) {
                    var color = Color.Red.Interpolate(Color.Green, bot.Chance);
                    bmp.SetPixel(bot.X, bot.Y, color);
                }

                var filename = Path.Combine(DestinyDirectory, $"{DestinyFilename}-heat.jpg");
                bmp.Save(filename);
            }
            SendText("Done");
        });
    }
}