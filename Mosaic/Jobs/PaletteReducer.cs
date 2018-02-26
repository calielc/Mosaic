using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.MachineLearning;
using ActivityQueue;
using Mosaic.Imaging;

namespace Mosaic.Jobs {
    internal sealed class PaletteReducer : IActivity {
        private readonly ImageCollection _images;
        private readonly Broadcast _broadcast;
        private const int MaxColor = 512;

        public PaletteReducer(ImageCollection images, Broadcast broadcast) {
            _images = images;
            _broadcast = broadcast;
        }

        public async Task Run() => await Task.Run(() => {
            _broadcast.Start(this, $"Creating new {MaxColor} color palette");
            try {
                var palette = ExtractPalette();

                BuildPalette(palette);
                UpdateImages(palette);
            }
            finally {
                _broadcast.End(this);
            }
        });

        private static void BuildPalette(IDictionary<RGBColor, RGBColor> palette) {
            var colors = new double[palette.Keys.Count][];

            Parallel.ForEach(palette.Keys.Select((color, index) => (color, index)), tuple => {
                colors[tuple.index] = tuple.color.CopyTo(new double[3]);
            });

            var kmeans = new KMeans(MaxColor);
            var clusters = kmeans.Learn(colors);

            var reduce = clusters.Centroids.Select(RGBColor.CopyFrom).ToArray();

            Parallel.ForEach(colors, color => {
                var normal = RGBColor.CopyFrom(color);
                var idx = clusters.Decide(color);

                palette[normal] = reduce[idx];
            });
        }

        private void UpdateImages(IReadOnlyDictionary<RGBColor, RGBColor> palette) {
            Parallel.ForEach(_images, image => {
                image.Reduce(palette);

                _broadcast.Step(this);
            });
        }

        private ConcurrentDictionary<RGBColor, RGBColor> ExtractPalette() {
            var result = new ConcurrentDictionary<RGBColor, RGBColor>();

            Parallel.ForEach(_images, image => {
                for (var x = 0; x < image.Width; x++) {
                    for (var y = 0; y < image.Height; y++) {
                        var color = image[x, y];
                        result.TryAdd(color, color);
                    }
                }

                _broadcast.Step(this);
            });

            return result;
        }
    }
}