using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Mosaic {
    [DebuggerDisplay("X: {X}, Y: {Y}, Color: {Color}, Chance: {Chance}")]
    internal class PixelBot {

        public PixelBot(int x, int y) {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public double Chance { get; private set; }
        public SingleColor Color { get; private set; }

        public void Load(int count, IEnumerable<SingleColor> colors) {
            var mixes = colors
                .GroupByDistance(16)
                .OrderByDescending(color => color.Count)
                .ToArray();

            var mostLikely = mixes.First();
            Color = mostLikely.Current;
            Chance = 1d * mostLikely.Count / count;
        }

        public void Save(Bitmap bmp) {
            bmp.SetPixel(X, Y, Color);
        }

#if DEBUG
        public override string ToString() => $"X: {X}, Y: {Y}, Color: {Color}, Chance: {Chance:p}";
#endif
    }
}