using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Mosaic {
    internal sealed class ConcurrentBitmapCollection {
        private readonly ConcurrentBag<ConcurrentBitmap> _images;

        public ConcurrentBitmapCollection(IEnumerable<string> filenames) {
            _images = new ConcurrentBag<ConcurrentBitmap>();
            Parallel.ForEach(filenames, filename => { _images.Add(new ConcurrentBitmap(filename)); });

            Count = _images.Count;

            var first = _images.First();
            Width = first.Width;
            Height = first.Height;
        }

        public int Count { get; }

        public int Width { get; }
        public int Height { get; }

        public IEnumerable<SingleColor> GetPixel(int x, int y)
            => _images.Select(image => image[x, y]);
    }
}