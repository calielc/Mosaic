using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace Mosaic
{
    internal class MixedColor
    {
        private List<Color> _colors = new List<Color>();

        public MixedColor(Color color)
        {
            _colors.Add(color);
            CalcBlend();
        }

        public Color Current { get; private set; }

        public int Count => _colors.Count();

        public void Add(params MixedColor[] mixes)
        {
            foreach (var mix in mixes)
            {
                _colors.AddRange(mix._colors);
            }
            CalcBlend();
        }

        private void CalcBlend()
        {
            var r = Convert.ToByte(_colors.Average(item => 1m * item.R));
            var g = Convert.ToByte(_colors.Average(item => 1m * item.G));
            var b = Convert.ToByte(_colors.Average(item => 1m * item.B));

            Current = Color.FromArgb(r, g, b);
        }
    }
}