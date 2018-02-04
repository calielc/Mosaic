using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mosaic.Layers;

namespace Mosaic {
    internal sealed class ImageDiscovery {
        private readonly IReadOnlyCollection<string> _filenames;

        public ImageDiscovery(string searchDirectory, string searchPattern) {
            SearchDirectory = searchDirectory;
            SearchPattern = searchPattern;

            _filenames = Directory.GetFiles(searchDirectory, searchPattern);
        }

        public string SearchDirectory { get; }
        public string SearchPattern { get; }

        public string FirstFilename => _filenames.First();

        public async Task<LayerCollection> Load() => await Task.Factory.StartNew(() => {
            double total = _filenames.Count;
            Broadcast.Start(this, $"Loading {_filenames.Count} images");
            try {
                var items = new ConcurrentBag<Image>();

                Parallel.ForEach(_filenames, filename => {
                    items.Add(new Image(filename));

                    Broadcast.Progress(this, items.Count / total);
                });

                return new LayerCollection(items);
            }
            finally {
                Broadcast.End(this);
            }
        });
    }
}