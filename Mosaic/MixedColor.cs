using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

namespace Mosaic
{
    [DebuggerDisplay("[R: {R}, G: {G}, B: {B}]")]
    public sealed class MixedColor : IEquatable<MixedColor>
    {
        private readonly Dictionary<Color, int> _colors;

        private MixedColor()
        {
            _colors = new Dictionary<Color, int>();
        }

        public MixedColor(Color color, int times = 1)
        {
            _colors = new Dictionary<Color, int>
            {
                [color] = times
            };
            Count = times;

            R = color.R;
            G = color.G;
            B = color.B;
        }

        internal MixedColor(params Color[] colors)
        {
            _colors = new Dictionary<Color, int>();
            foreach (var color in colors)
            {
                IncreaseColor(color, 1);
            }

            UpdateCurrent();
        }

        public double R { get; private set; }
        public double G { get; private set; }
        public double B { get; private set; }
        public Color Current => Color.FromArgb(Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B));

        public int Count { get; private set; }

        public void Add(Color color, int times = 1)
        {
            IncreaseColor(color, times);
            UpdateCurrent();
        }

        public void Add(MixedColor other)
        {
            foreach (var pair in other._colors)
            {
                IncreaseColor(pair.Key, pair.Value);
            }
            UpdateCurrent();
        }

        private void IncreaseColor(Color color, int times)
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
                var color = _colors.Keys.Single();
                R = color.R;
                G = color.G;
                B = color.B;
            }
            else
            {
                double total = _colors.Values.Sum();
                R = _colors.Sum(item => item.Value * item.Key.R) / total;
                G = _colors.Sum(item => item.Value * item.Key.G) / total;
                B = _colors.Sum(item => item.Value * item.Key.B) / total;
            }
        }

        public double Distance(MixedColor other)
        {
            var r = R - other.R;
            var g = G - other.G;
            var b = B - other.B;

            return Math.Sqrt(r * r + g * g + b * b);
        }

        public bool Equals(MixedColor other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (_colors.Count != other._colors.Count) return false;
            if (_colors.Keys.Except(other._colors.Keys).Any()) return false;
            return _colors.All(color => color.Value == other._colors[color.Key]);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj.GetType() == GetType() && Equals((MixedColor)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Current.GetHashCode();
                hashCode = (hashCode * 397) ^ Count;
                hashCode = (hashCode * 397) ^ _colors.GetHashCode();
                return hashCode;
            }
        }

        public static implicit operator MixedColor(Color color) => new MixedColor(color);

        public static implicit operator Color(MixedColor mix) => mix.Current;

        public static MixedColor operator +(MixedColor mix1, MixedColor mix2)
        {
            var result = new MixedColor();
            foreach (var pair in mix1._colors.Concat(mix2._colors))
            {
                result.IncreaseColor(pair.Key, pair.Value);
            }
            result.UpdateCurrent();

            return result;
        }

        public static MixedColor operator +(MixedColor mix, Color color)
        {
            var result = new MixedColor();
            foreach (var pair in mix._colors)
            {
                result.IncreaseColor(pair.Key, pair.Value);
            }
            result.IncreaseColor(color, 1);
            result.UpdateCurrent();

            return result;
        }

        public static MixedColor operator +(Color color, MixedColor mix)
        {
            var result = new MixedColor();
            result.IncreaseColor(color, 1);
            foreach (var pair in mix._colors)
            {
                result.IncreaseColor(pair.Key, pair.Value);
            }
            result.UpdateCurrent();

            return result;
        }

        public static bool operator ==(MixedColor left, MixedColor right) => Equals(left, right);

        public static bool operator !=(MixedColor left, MixedColor right) => !Equals(left, right);

#if DEBUG
        public override string ToString() => $"[R: {R:0.00}, G: {G:0.00}, B: {B:0.00}]";
#endif
    }
}