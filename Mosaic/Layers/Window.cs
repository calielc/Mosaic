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

        public RGBColor this[int x, int y] => Image[Left + x, Top + y];
    }
}