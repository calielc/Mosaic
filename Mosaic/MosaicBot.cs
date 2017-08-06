using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Mosaic
{
    internal partial class MosaicBot
    {
        private int _x;
        private int _y;
        private IEnumerable<ConcurrentBitmap> _images;
        private MixedColor[] _mixes;

        public MosaicBot(int x, int y, IEnumerable<ConcurrentBitmap> images)
        {
            _x = x;
            _y = y;
            _images = images;
        }

        public Color Color { get; private set; } = Color.Black;

        internal void Process()
        {
            var colors = _images
                .Select(image => image.GetPixel(_x, _y))
                .ToArray();

            _mixes = colors
                .Select(color => new MixedColor(color))
                .GroupByDistance(1)
                .GroupByDistance(2)
                .GroupByDistance(4)
                .GroupByDistance(8)
                .GroupByDistance(16)
                .OrderByDescending(mix => mix.Count)
                .Where(mix => mix.Count > 1)
                .Take(5)
                .ToArray();
        }

        private Color GetColor()
        {
            var sum = _mixes.Sum(mix => mix.Count);
            var probableMix = _mixes.First();

            var trust = 1m * probableMix.Count / sum;
            if (trust >= 0.5m)
            {
                return probableMix.Current;
            }
            return Color.Fuchsia;
        }

        internal void Build(Bitmap bmp)
        {
            bmp.SetPixel(_x, _y, GetColor());
        }
    }
}