using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Mosaic.Imaging;

namespace Mosaic.Jobs {
    [DebuggerDisplay("Count: {_count}, R: {_r.Count}, G: {_g.Count}, B: {_b.Count}")]
    internal sealed class ColorHistogram {
        private readonly int _count;
        private readonly ConcurrentDictionary<byte, int> _r = new ConcurrentDictionary<byte, int>();
        private readonly ConcurrentDictionary<byte, int> _g = new ConcurrentDictionary<byte, int>();
        private readonly ConcurrentDictionary<byte, int> _b = new ConcurrentDictionary<byte, int>();

        public ColorHistogram(IEnumerable<RGBColor> values) {
            _count = 0;
            foreach (var value in values) {
                _count++;
                _r.AddOrUpdate(value.R, _ => 1, (_, current) => current + 1);
                _g.AddOrUpdate(value.G, _ => 1, (_, current) => current + 1);
                _b.AddOrUpdate(value.B, _ => 1, (_, current) => current + 1);
            }
        }

        public static double operator %(ColorHistogram left, ColorHistogram right) {
            Debug.Assert(left._count == right._count, "left._count == right._count");

            const double oneThird = 1d / 3d;
            var r = CalcDelta(in left._r, in right._r, left._count) * oneThird;
            var g = CalcDelta(in left._g, in right._g, left._count) * oneThird;
            var b = CalcDelta(in left._b, in right._b, left._count) * oneThird;

            return r + g + b;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static double CalcDelta(in ConcurrentDictionary<byte, int> left, in ConcurrentDictionary<byte, int> right, double count) {
            var result = 0d;

            foreach (var key in left.Keys.Union(right.Keys)) {
                left.TryGetValue(key, out var valueLeft);
                right.TryGetValue(key, out var valueRight);

                result += Math.Abs(valueLeft - valueRight);
            }

            return 100d * (1d - result / count);
        }
    }
}