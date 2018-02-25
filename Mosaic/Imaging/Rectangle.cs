using System.Diagnostics;

namespace Mosaic.Imaging {
    [DebuggerDisplay("Left: {Left}, Top: {Top}, Width: {Width}, Height: {Height}")]
    internal sealed class Rect : IRectangle {
        private readonly int _left;
        private readonly int _top;
        private readonly int _width;
        private readonly int _height;

        public Rect(int left, int top, int width, int height) {
            _left = left;
            _top = top;
            _width = width;
            _height = height;
        }

        public int Left => _left;
        public int Top => _top;

        public int Width => _width;
        public int Height => _height;
    }
}