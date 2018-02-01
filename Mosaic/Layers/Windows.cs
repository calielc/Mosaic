using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mosaic.Layers {
    internal sealed class Windows : ILayers {
        private readonly IRectangle _rectangle;
        private readonly LinkedList<Window> _items;

        public Windows(IRectangle rectangle, IEnumerable<Window> items) {
            _rectangle = rectangle;
            _items = new LinkedList<Window>(items.OrderBy(item => item.Name));
        }

        public int Left => _rectangle.Left;
        public int Top => _rectangle.Top;

        public int Width => _rectangle.Width;
        public int Height => _rectangle.Height;

        public IEnumerable<Windows> SplitBy(int tilesX, int tilesY) {
            var sizeX = 1d * Width / tilesX;
            var sizeY = 1d * Height / tilesY;

            for (var x = 0; x < tilesX; x++) {
                for (var y = 0; y < tilesY; y++) {
                    var left = Multiply(x, sizeX);
                    var width = Multiply(x + 1, sizeX) - left;

                    var top = Multiply(y, sizeY);
                    var height = Multiply(y + 1, sizeY) - top;

                    var rectangle = new Rect(Left + left, Top + top, width, height);
                    var items = _items.Select(item => new Window(item.Image, rectangle));
                    yield return new Windows(rectangle, items);
                }
            }
        }

        private static int Multiply(double number, double times) => Convert.ToInt32(Math.Round(number * times));

        int IReadOnlyCollection<ILayer>.Count => _items.Count;
        IEnumerator<ILayer> IEnumerable<ILayer>.GetEnumerator() => _items.GetEnumerator();

        public IEnumerator GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }
}