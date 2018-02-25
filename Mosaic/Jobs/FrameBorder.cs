using System.Diagnostics;
using System.Linq;
using Mosaic.Imaging;

namespace Mosaic.Jobs {
    [DebuggerDisplay("Rect: {_rectangle}, Length: {_colors.Length}")]
    internal sealed class FrameBorder {
        private readonly IRectangle _rectangle;
        private readonly RGBColor[] _colors;

        private FrameBorder(PixelIndexer image, IRectangle rectangle) {
            _rectangle = rectangle;
            _colors = image[rectangle].ToArray();
        }

        public static double operator %(FrameBorder self, FrameBorder other) {
            Debug.Assert(self._colors.Length == other._colors.Length, "left._colors.Length == right._colors.Length");

            var result = 0d;
            var length = self._colors.Length;
            for (var idx = 0; idx < length; idx++) {
                var leftColor = self._colors[idx];
                var rightColor = other._colors[idx];
                result += leftColor - rightColor;
            }

            return 100d * (1d - result / (RGBColor.MaxDelta * self._colors.Length));
        }

        public static FrameBorder FromLeft(Image image, IRectangle rect) {
            var borderRect = new Rect(rect.Left, rect.Top, 1, rect.Height);
            return new FrameBorder(image.Reduced, borderRect);
        }

        public static FrameBorder FromRight(Image image, IRectangle rect) {
            var borderRect = new Rect(rect.Left + rect.Width - 1, rect.Top, 1, rect.Height);
            return new FrameBorder(image.Reduced, borderRect);
        }

        public static FrameBorder FromTop(Image image, IRectangle rect) {
            var borderRect = new Rect(rect.Left, rect.Top, rect.Width, 1);
            return new FrameBorder(image.Reduced, borderRect);
        }

        public static FrameBorder FromBottom(Image image, IRectangle rect) {
            var borderRect = new Rect(rect.Left, rect.Top + rect.Height - 1, rect.Width, 1);
            return new FrameBorder(image.Reduced, borderRect);
        }
    }
}