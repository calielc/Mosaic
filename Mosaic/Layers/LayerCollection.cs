using System;
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

        public LayerCollection(IEnumerable<Window> windows) {
            _layers = new LinkedList<ILayer>(windows.OrderBy(image => image.Name));
            _rectangle = _layers.First.Value;

            _rawImages = new LinkedList<Image>(_layers.Cast<Window>().Select(window => window.Image));
        }

        public int Left => _rectangle.Left;
        public int Top => _rectangle.Top;

        public int Width => _rectangle.Width;
        public int Height => _rectangle.Height;

        public int Count => _layers.Count;

        public IEnumerable<LayerCollection> SplitBy(int tiles) {
            return
            (
                from h in Split(_rectangle.Width, tiles)
                from v in Split(_rectangle.Height, tiles)
                select new Rect(_rectangle.Left + h.start, _rectangle.Top + v.start, h.size, v.size)
            ).Select(rect => new LayerCollection(_rawImages.Select(image => new Window(image, rect))));

            IEnumerable<(int start, int size)> Split(double totalSize, int count) {
                var localSize = totalSize / count;
                return Enumerable.Range(0, count).Select(i => {
                    var start = localSize * i;
                    var end = localSize * (i + 1);

                    return (Convert.ToInt32(Math.Round(start)), Convert.ToInt32(Math.Round(end - start)));
                });
            }
        }

        IEnumerator<ILayer> IEnumerable<ILayer>.GetEnumerator() => _layers.Cast<ILayer>().GetEnumerator();

        public IEnumerator GetEnumerator() => ((IEnumerable)_layers).GetEnumerator();
    }
}