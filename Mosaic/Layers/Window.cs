namespace Mosaic.Layers {
    internal sealed class Window : ILayer {
        private readonly IRectangle _rectangle;
        private readonly Image _image;
        private readonly string _name;

        public Window(Image image, IRectangle rectangle) {
            _image = image;
            _rectangle = rectangle;

            _name = $"{_image.Name}-{_rectangle.Left}X{_rectangle.Top}-{_rectangle.Width}X{_rectangle.Height}";
        }

        public Image Image => _image;

        public string Name => _name;

        public int Left => _rectangle.Left;
        public int Top => _rectangle.Top;

        public int Width => _rectangle.Width;
        public int Height => _rectangle.Height;

        public RGBColor this[int x, int y] => _image[_rectangle.Left + x, _rectangle.Top + y];
    }
}