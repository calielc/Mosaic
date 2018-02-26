using System.Collections.Generic;

namespace Mosaic.Extensions {
    public static class EnumerableExtensions {
        public static double HarmonicAverage(this IEnumerable<double> self) {
            var sum = 0d;
            var count = 0;

            foreach (var value in self) {
                sum += 1d / value;
                count++;
            }

            return count / sum;
        }
    }
}