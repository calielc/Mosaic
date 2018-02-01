using System.Collections.Generic;

namespace Mosaic.Layers {
    internal sealed class Window : ILayer {
        private readonly IRectangle _rectangle;

        public Window(Image image, IRectangle rectangle) {
            Image = image;
            _rectangle = rectangle;

            Name = $"{Image.Name}-{_rectangle.Left}X{_rectangle.Top}-{_rectangle.Width}X{_rectangle.Height}";
        }

        public Image Image { get; }

        public string Name { get; }

        public int Left => _rectangle.Left;
        public int Top => _rectangle.Top;

        public int Width => _rectangle.Width;
        public int Height => _rectangle.Height;

        public IEnumerable<Pixel> GetPixels() {
            for (int x = 0, realX = _rectangle.Left; x < _rectangle.Width; x++, realX++) {
                for (int y = 0, realY = _rectangle.Top; y < _rectangle.Height; y++, realY++) {
                    yield return new Pixel(x, y, Image[realX, realY]);
                }
            }
        }
    }
}