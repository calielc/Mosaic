using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mosaic.Imaging {
    internal sealed class ImageDiscovery {
        private readonly Broadcast _broadcast;

        public ImageDiscovery(Broadcast broadcast) {
            _broadcast = broadcast;
        }

        public string SearchDirectory { get; set; }
        public string SearchPattern { get; set; }

        public string FirstFilename { get; private set; }

        public async Task<ImageCollection> Load() => await Task.Factory.StartNew(() => {
            var filenames = Directory.GetFiles(SearchDirectory, SearchPattern);

            FirstFilename = filenames.First();

            double total = filenames.Length;
            _broadcast.Start(this, $"Loading {filenames.Length} images");
            try {
                var items = new ConcurrentBag<Image>();

                Parallel.ForEach(filenames, filename => {
                    items.Add(new Image(filename));

                    _broadcast.Progress(this, items.Count / total);
                });

                return new ImageCollection(items);
            }
            finally {
                _broadcast.End(this);
            }
        });
    }
}