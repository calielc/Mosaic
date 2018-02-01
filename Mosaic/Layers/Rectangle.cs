using System.Diagnostics;

namespace Mosaic.Layers {
    [DebuggerDisplay("Left: {Left}, Top: {Top}, Width: {Width}, Height: {Height}")]
    public readonly struct Rect : IRectangle {
        public Rect(int left, int top, int width, int height) {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public int Left { get; }
        public int Top { get; }

        public int Width { get; }
        public int Height { get; }
    }
}