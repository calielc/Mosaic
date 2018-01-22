using System;
using System.Diagnostics;
using System.Drawing;

namespace Mosaic {
    [DebuggerDisplay("[R: {R}, G: {G}, B: {B}]")]
    public struct SingleColor : IEquatable<SingleColor> {
        public SingleColor(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
        }

        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public bool Equals(SingleColor other) => R == other.R && G == other.G && B == other.B;

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            return obj is SingleColor color && Equals(color);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                return hashCode;
            }
        }

#if DEBUG
        public override string ToString() => $"[R: {R}, G: {G}, B: {B}]";
#endif

        public static bool operator ==(SingleColor left, SingleColor right) => left.Equals(right);

        public static bool operator !=(SingleColor left, SingleColor right) => !left.Equals(right);

        public static implicit operator Color(SingleColor self) => Color.FromArgb(self.R, self.G, self.B);

        public static implicit operator SingleColor(Color self) => new SingleColor(self.R, self.G, self.B);
    }
}