using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Mosaic {
    [DebuggerDisplay("{Count} x [R: {R}, G: {G}, B: {B}]")]
    public sealed class MixedColor : IEquatable<MixedColor> {
        private readonly Dictionary<SingleColor, int> _colors;

        public MixedColor(SingleColor color, int times = 1) {
            _colors = new Dictionary<SingleColor, int> {
                [color] = times
            };

            Count = times;

            R = color.R;
            G = color.G;
            B = color.B;
        }

        public double R { get; private set; }
        public double G { get; private set; }
        public double B { get; private set; }
        public SingleColor Current => new SingleColor(Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B));

        public int Count { get; private set; }

        public MixedColor Add(SingleColor color, int times = 1) {
            IncreaseColor(color, times);
            UpdateCurrent();

            return this;
        }

        public MixedColor Add(MixedColor other) {
            IncreaseColor(other);
            UpdateCurrent();

            return this;
        }

        private void IncreaseColor(SingleColor color, int times) {
            if (_colors.ContainsKey(color)) {
                _colors[color] += times;
            }
            else {
                _colors[color] = times;
            }
            Count += times;
        }

        private void IncreaseColor(MixedColor other) {
            foreach (var item in other._colors) {
                if (_colors.ContainsKey(item.Key)) {
                    _colors[item.Key] += item.Value;
                }
                else {
                    _colors[item.Key] = item.Value;
                }
            }
            Count += other.Count;
        }

        private void UpdateCurrent() {
            if (_colors.Count == 1) {
                var color = _colors.Keys.Single();
                R = color.R;
                G = color.G;
                B = color.B;
            }
            else {
                double total = Count;
                R = _colors.Sum(item => item.Value * item.Key.R) / total;
                G = _colors.Sum(item => item.Value * item.Key.G) / total;
                B = _colors.Sum(item => item.Value * item.Key.B) / total;
            }
        }

        public double Distance(MixedColor other) {
            var r = R - other.R;
            var g = G - other.G;
            var b = B - other.B;

            return Math.Sqrt(r * r + g * g + b * b);
        }

        public bool Equals(MixedColor other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (Current != other.Current) return false;

            if (_colors.Count != other._colors.Count) return false;
            if (_colors.Keys.Except(other._colors.Keys).Any()) return false;
            return _colors.All(color => color.Value == other._colors[color.Key]);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj.GetType() == GetType() && Equals((MixedColor) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = Current.GetHashCode();
                hashCode = (hashCode * 397) ^ Count.GetHashCode();
                hashCode = (hashCode * 397) ^ _colors.GetHashCode();
                return hashCode;
            }
        }

        public static explicit operator MixedColor(SingleColor color) => new MixedColor(color);

        public static explicit operator SingleColor(MixedColor mix) => mix.Current;

        public static bool operator ==(MixedColor left, MixedColor right) => Equals(left, right);

        public static bool operator !=(MixedColor left, MixedColor right) => !Equals(left, right);

#if DEBUG
        public override string ToString() => $"{Count} x [R: {R:0.00}, G: {G:0.00}, B: {B:0.00}]";
#endif
    }
}