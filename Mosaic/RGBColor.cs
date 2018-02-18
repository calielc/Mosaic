using System;
using System.Diagnostics;
using System.Drawing;

namespace Mosaic {
    [DebuggerDisplay("[R: {_r}, G: {_g}, B: {_b}]")]
    public readonly struct RGBColor : IEquatable<RGBColor> {
        private readonly byte _b;
        private readonly byte _g;
        private readonly byte _r;

        public RGBColor(byte r, byte g, byte b) {
            _r = r;
            _g = g;
            _b = b;
        }

        public byte R => _r;
        public byte G => _g;
        public byte B => _b;

        public bool Equals(RGBColor other) => _r == other._r && _g == other._g && _b == other._b;

        public override bool Equals(object obj) {
            if (obj is null) {
                return false;
            }

            return obj is RGBColor color && Equals(color);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = _r.GetHashCode();
                hashCode = (hashCode * 397) ^ _g.GetHashCode();
                hashCode = (hashCode * 397) ^ _b.GetHashCode();
                return hashCode;
            }
        }

#if DEBUG
        public override string ToString() => $"[R: {_r}, G: {_g}, B: {_b}]";
#endif

        public static bool operator ==(RGBColor left, RGBColor right) => left.Equals(right);

        public static bool operator !=(RGBColor left, RGBColor right) => !left.Equals(right);

        public static implicit operator Color(RGBColor self) => Color.FromArgb(self.R, self.G, self.B);

        public static implicit operator RGBColor(Color self) => new RGBColor(self.R, self.G, self.B);

        public double[] CopyTo(double[] array) {
            array[0] = _r;
            array[1] = _g;
            array[2] = _b;
            return array;
        }

        public static RGBColor CopyFrom(double[] array) => new RGBColor((byte)array[0], (byte)array[1], (byte)array[2]);
    }
}