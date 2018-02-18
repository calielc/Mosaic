using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mosaic.Layers {
    internal sealed class LayerCollection : IRectangle, IReadOnlyCollection<ILayer> {
        private readonly LinkedList<Image> _rawImages;
        private readonly LinkedList<ILayer> _layers;
        private readonly IRectangle _rectangle;

        public LayerCollection(IEnumerable<Image> images) {
            _rawImages = new LinkedList<Image>(images.OrderBy(image => image.Name));

            _layers = new LinkedList<ILayer>(_rawImages);
            _rectangle = _layers.First.Value;
        }

        private LayerCollection(IEnumerable<Window> windows) {
            _layers = new LinkedList<ILayer>(windows.OrderBy(image => image.Name));
            _rectangle = _layers.First.Value;

            _rawImages = new LinkedList<Image>(_layers.Cast<Window>().Select(window => window.Image));
        }

        public int Left => _rectangle.Left;
        public int Top => _rectangle.Top;

        public int Width => _rectangle.Width;
        public int Height => _rectangle.Height;

        public int Count => _layers.Count;

        public IReadOnlyCollection<Image> AsImages => _rawImages;

        public IEnumerable<LayerCollection> SplitBy(int tiles) {
            return
            (
                from h in Split(_rectangle.Width, tiles)
                from v in Split(_rectangle.Height, tiles)
                select new Rect(_rectangle.Left + h.start, _rectangle.Top + v.start, h.size, v.size)
            ).Select(rect => new LayerCollection(_rawImages.Select(image => new Window(image, rect))));

            IEnumerable<(int start, int size)> Split(int totalSize, int count) {
                var span = 0;
                while (count > 0) {
                    var size = (totalSize - span) / count;

                    yield return (span, size);

                    span += size;
                    count--;
                }
            }
        }

        IEnumerator<ILayer> IEnumerable<ILayer>.GetEnumerator() => _layers.Cast<ILayer>().GetEnumerator();

        public IEnumerator GetEnumerator() => ((IEnumerable)_layers).GetEnumerator();
    }
}