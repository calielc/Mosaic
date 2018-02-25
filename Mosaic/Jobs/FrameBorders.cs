using Mosaic.Imaging;

namespace Mosaic.Jobs {
    internal readonly struct FrameBorders {
        public FrameBorders(Image image, IRectangle rect) {
            Left = FrameBorder.FromLeft(image, rect);
            Right = FrameBorder.FromRight(image, rect);
            Top = FrameBorder.FromTop(image, rect);
            Bottom = FrameBorder.FromBottom(image, rect);
        }

        public FrameBorder Bottom { get; }

        public FrameBorder Top { get; }

        public FrameBorder Right { get; }

        public FrameBorder Left { get; }

        public static double operator %(FrameBorders self, FrameBorders other) {
            var left = 1d / (self.Left % other.Left);
            var right = 1d / (self.Right % other.Right);
            var top = 1d / (self.Top % other.Top);
            var bottom = 1d / (self.Bottom % other.Bottom);

            return 4d / (left + right + top + bottom);
        }
    }
}