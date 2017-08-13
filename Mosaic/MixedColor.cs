using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

namespace Mosaic
{
    [DebuggerDisplay("Current: {Current}, Count: {Count}")]
    internal class MixedColor
    {
        private readonly Dictionary<Color, int> _colors;

        public MixedColor(Color color) : this(color, 1) { }

        public MixedColor(Color color, int times)
        {
            _colors = new Dictionary<Color, int>
            {
                [color] = times
            };

            Count = times;
            Current = color;
        }

        public Color Current { get; private set; }

        public int Count { get; private set; }

        public void Add(Color color)
        {
            IncreaseColor(color);
            UpdateCurrent();
        }

        public void Add(MixedColor mix)
        {
            foreach (var pair in mix._colors)
            {
                IncreaseColor(pair.Key, pair.Value);
            }
            UpdateCurrent();
        }

        private void IncreaseColor(Color color, int times = 1)
        {
            if (_colors.ContainsKey(color))
            {
                _colors[color] += times;
            }
            else
            {
                _colors[color] = times;
            }
            Count += times;
        }

        private void UpdateCurrent()
        {
            if (_colors.Count == 1)
            {
                Current = _colors.Keys.Single();
                return;
            }

            double total = _colors.Values.Sum();
            var r = Convert.ToByte(_colors.Sum(item => item.Value * item.Key.R) / total);
            var g = Convert.ToByte(_colors.Sum(item => item.Value * item.Key.G) / total);
            var b = Convert.ToByte(_colors.Sum(item => item.Value * item.Key.B) / total);

            Current = Color.FromArgb(r, g, b);
        }
    }
}