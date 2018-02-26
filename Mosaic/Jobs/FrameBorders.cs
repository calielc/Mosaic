using Mosaic.Extensions;
using Mosaic.Imaging;

namespace Mosaic.Jobs {
    internal readonly struct FrameBorders : IDirection<FrameBorder> {
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

        public static double operator %(FrameBorders self, FrameBorders other)
            => new[] {
                self.Left % other.Left,
                self.Right % other.Right,
                self.Top % other.Top,
                self.Bottom % other.Bottom,
            }.HarmonicAverage();
    }
}