using System.Diagnostics;

namespace Mosaic {
    [DebuggerDisplay("X: {X}, Y: {Y}, Color: {Color.R}-{Color.G}-{Color.B}")]
    public readonly struct Pixel {
        public Pixel(int x, int y, RGBColor color) {
            X = x;
            Y = y;
            Color = color;
        }

        public int X { get; }
        public int Y { get; }

        public RGBColor Color { get; }
    }
}