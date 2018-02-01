using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mosaic.Layers;

namespace Mosaic {
    internal sealed class ImageDiscovery {
        private readonly IReadOnlyCollection<string> _filenames;
        private Images _images;

        public ImageDiscovery(string searchDirectory, string searchPattern) {
            SearchDirectory = searchDirectory;
            SearchPattern = searchPattern;

            Broadcast.Start(this, $"Searching folder {SearchDirectory}\\{SearchPattern}...");
            _filenames = Directory.GetFiles(searchDirectory, searchPattern);
            Broadcast.End(this);
        }

        public string SearchDirectory { get; }
        public string SearchPattern { get; }

        public int Count => _filenames.Count;

        public string FirstFilename => _filenames.First();

        public Images Load() {
            if (_images != null) {
                return _images;
            }

            Broadcast.Start(this, $"Loading {_filenames.Count} images");

            var images = new ConcurrentBag<Image>();
            Parallel.ForEach(_filenames, filename => {
                var image = new Image(filename);
                images.Add(image);

                Broadcast.Progress(this, 1d * images.Count / _filenames.Count);
            });

            Broadcast.End(this);

            return _images = new Images(images);
        }
    }
}