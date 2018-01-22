using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mosaic {
    internal sealed class ConcurrentBitmapCollection : IReadOnlyCollection<ConcurrentBitmap> {
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

        public IEnumerator<ConcurrentBitmap> GetEnumerator() => _images.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_images).GetEnumerator();
    }
}