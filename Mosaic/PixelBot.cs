using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Mosaic
{
    [DebuggerDisplay("X: {_x}, Y: {_y}, Color: {Color}, Chance: {Chance}")]
    internal partial class PixelBot
    {
        private int _x;
        private int _y;
        private ConcurrentBitmapCollection _images;

        public PixelBot(int x, int y, ConcurrentBitmapCollection images)
        {
            _x = x;
            _y = y;
            _images = images;
        }

        public int X => _x;
        public int Y => _y;

        public double Chance { get; private set; }
        public Color Color { get; private set; }

        public Func<PixelBot, IEnumerable<PixelBot>> GetNeighbours { get; set; }

        public void Load()
        {
            var mixes = _images.GetPixel(_x, _y)
                .GroupByDistance(16)
                .OrderByDescending(color => color.Count)
                .ToArray();

            var mostLikely = mixes.First();

            Color = mostLikely.Current;
            Chance = 1d * mostLikely.Count / _images.Count;
        }

        public void Save(Bitmap bmp)
        {
            bmp.SetPixel(_x, _y, Color);
        }

        public double NeighbourDistance(PixelBot bot)
        {
            if (bot == this)
            {
                return 0;
            }

            return Math.Sqrt(Extensions.DifferenceSqr(bot._x, this._x) + Extensions.DifferenceSqr(bot._y, this._y));
        }

        public override string ToString() => $"X: {_x}, Y: {_y}, Color: {Color}, Chance: {Chance:p}";
    }
}