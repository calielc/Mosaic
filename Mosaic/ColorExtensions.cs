using System.Drawing;

namespace Mosaic {
    internal static class ColorExtensions {
        public static Color Interpolate(this Color source, Color target, double percent) {
            var r = (byte)(source.R + (target.R - source.R) * percent);
            var g = (byte)(source.G + (target.G - source.G) * percent);
            var b = (byte)(source.B + (target.B - source.B) * percent);

            return Color.FromArgb(r, g, b);
        }
    }
}