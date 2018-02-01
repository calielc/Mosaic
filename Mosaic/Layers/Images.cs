using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mosaic.Layers {
    internal sealed class Images : ILayers {
        private readonly LinkedList<Image> _items;

        public Images(IEnumerable<Image> images) {
            _items = new LinkedList<Image>(images.OrderBy(layer => layer.Name));

            var image = _items.First.Value;
            Width = image.Width;
            Height = image.Height;
        }

        public int Width { get; }
        public int Height { get; }

        public int Count => _items.Count;

        public IEnumerable<Windows> SplitBy(int tilesX, int tilesY) {
            var sizeX = 1d * Width / tilesX;
            var sizeY = 1d * Height / tilesY;

            for (var x = 0; x < tilesX; x++) {
                for (var y = 0; y < tilesY; y++) {
                    var left = Multiply(x, sizeX);
                    var width = Multiply(x + 1, sizeX) - left;

                    var top = Multiply(y, sizeY);
                    var height = Multiply(y + 1, sizeY) - top;

                    var rectangle = new Rect(left, top, width, height);
                    var items = _items.Select(item => new Window(item, rectangle));
                    yield return new Windows(rectangle, items);
                }
            }
        }

        private static int Multiply(double number, double times) => Convert.ToInt32(Math.Round(number * times));

        int IRectangle.Left => 0;
        int IRectangle.Top => 0;

        IEnumerator<ILayer> IEnumerable<ILayer>.GetEnumerator() => _items.GetEnumerator();

        public IEnumerator GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }
}