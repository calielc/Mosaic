using Mosaic.Layers;

namespace Mosaic.Bots {
    internal readonly struct BotResult : IRectangle {
        private readonly IRectangle _rectangle;

        public BotResult(IRectangle rectangle, RGBColor[,] colors, double[,] odds) {
            _rectangle = rectangle;
            Colors = colors;
            Odds = odds;
        }

        public int Left => _rectangle.Left;
        public int Top => _rectangle.Top;

        public int Width => _rectangle.Width;
        public int Height => _rectangle.Height;

        public RGBColor[,] Colors { get; }
        public double[,] Odds { get; }
    }
}