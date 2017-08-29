using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Mosaic
{
    [DebuggerDisplay("X: {X}, Y: {Y}, Color: {Color}, Chance: {Chance}")]
    internal class PixelBot
    {
        private readonly ConcurrentBitmapCollection _images;

        public PixelBot(int x, int y, ConcurrentBitmapCollection images)
        {
            X = x;
            Y = y;
            _images = images;
        }

        public int X { get; }
        public int Y { get; }

        public double Chance { get; private set; }
        public Color Color { get; private set; }

        public Func<PixelBot, IEnumerable<PixelBot>> GetNeighbours { get; set; }

        public void Load()
        {
            var mixes = _images.GetPixel(X, Y)
                .GroupByDistance(1)
                .OrderByDescending(color => color.Count)
                .ToArray();

            var mostLikely = mixes.First();

            Color = mostLikely.Current;
            Chance = 1d * mostLikely.Count / _images.Count;
        }

        public void Save(Bitmap bmp)
        {
            bmp.SetPixel(X, Y, Color);
        }

        public double NeighbourDistance(PixelBot bot)
        {
            if (bot == this)
            {
                return 0;
            }

            return Math.Sqrt(
                MathUtils.DifferenceSqr(bot.X, this.X) +
                MathUtils.DifferenceSqr(bot.Y, this.Y));
        }

        public override string ToString() => $"X: {X}, Y: {Y}, Color: {Color}, Chance: {Chance:p}";
    }
}