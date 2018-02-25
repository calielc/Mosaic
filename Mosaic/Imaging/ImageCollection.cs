using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mosaic.Imaging {
    [DebuggerDisplay("Count: {Count}, Width: {Width}: Height: {Height}")]
    internal sealed class ImageCollection : ISize, IReadOnlyList<Image> {
        private readonly ISize _size;
        private readonly List<Image> _images;

        public ImageCollection(IEnumerable<Image> images) {
            _images = new List<Image>(images.OrderBy(image => image.Name));
            _size = _images.First();

            if (_images.All(image => image.Width == _size.Width) == false) {
                throw new Exception(nameof(_size.Width));
            }
            if (_images.All(image => image.Height == _size.Height) == false) {
                throw new Exception(nameof(_size.Height));
            }
        }

        public int Width => _size.Width;
        public int Height => _size.Height;

        public int Count => _images.Count;

        public Image this[int index] => _images[index];

        IEnumerator<Image> IEnumerable<Image>.GetEnumerator() => _images.GetEnumerator();

        public IEnumerator GetEnumerator() => ((IEnumerable)_images).GetEnumerator();
    }
}