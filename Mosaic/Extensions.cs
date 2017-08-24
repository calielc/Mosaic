using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mosaic
{
    internal static class Extensions
    {
        public static IEnumerable<MixedColor> GroupByDistance(this IEnumerable<Color> self, double maxDistance)
        {
            var sourceCollection = self.GroupBy(color => color)
                .Select(color => new MixedColor(color.Key, color.Count()))
                .OrderByDescending(color => color.Count)
                .ToArray();

            var resultCollection = new List<MixedColor>
            {
                sourceCollection.First()
            };
            foreach (var sourceItem in sourceCollection.Skip(1))
            {
                var matchItem = resultCollection
                    .Select(resultItem => new
                    {
                        Mix = resultItem,
                        Distance = ColorDistance(resultItem.Current, sourceItem.Current)
                    })
                    .Where(group => group.Distance < maxDistance)
                    .OrderBy(group => group.Distance)
                    .Select(group => group.Mix)
                    .FirstOrDefault();

                if (matchItem == null)
                {
                    resultCollection.Add(sourceItem);
                }
                else
                {
                    matchItem.Add(sourceItem);
                }
            }
            return resultCollection;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ColorDistance(Color c1, Color c2)
        {
            if (c1 == c2)
            {
                return 0;
            }

            var r = DifferenceSqr(c1.R, c2.R);
            var g = DifferenceSqr(c1.G, c2.G);
            var b = DifferenceSqr(c1.B, c2.B);
            return Math.Sqrt(r + g + b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DifferenceSqr(int value1, int value2)
        {
            if (value1 == value2)
            {
                return 0;
            }

            double value = value1 - value2;
            return value * value;
        }

        public static Color Interpolate(Color source, Color target, double percent)
        {
            var r = (byte)(source.R + (target.R - source.R) * percent);
            var g = (byte)(source.G + (target.G - source.G) * percent);
            var b = (byte)(source.B + (target.B - source.B) * percent);

            return Color.FromArgb(255, r, g, b);
        }
    }
}