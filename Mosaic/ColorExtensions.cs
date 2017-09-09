using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Mosaic {
    internal static class ColorExtensions {
        public static IEnumerable<MixedColor> GroupByDistance(this IEnumerable<SingleColor> self, double maxDistance) {
            var sourceCollection = self
                .GroupBy(color => color)
                .Select(color => new MixedColor(color.Key, color.Count()))
                .OrderByDescending(color => color.Count)
                .ToArray();

            var resultCollection = new List<MixedColor> {
                sourceCollection.First()
            };

            foreach (var sourceItem in sourceCollection.Skip(1)) {
                var matchItem = resultCollection
                    .Select(resultItem => new {
                        mix = resultItem,
                        distance = resultItem.Distance(sourceItem)
                    })
                    .Where(group => group.distance <= maxDistance + 1d)
                    .OrderBy(group => group.distance)
                    .FirstOrDefault()?.mix;

                if (matchItem == null) {
                    resultCollection.Add(sourceItem);
                }
                else {
                    matchItem.Add(sourceItem);
                }
            }

            return resultCollection;
        }

        public static Color Interpolate(this Color source, Color target, double percent) {
            var r = (byte) (source.R + (target.R - source.R) * percent);
            var g = (byte) (source.G + (target.G - source.G) * percent);
            var b = (byte) (source.B + (target.B - source.B) * percent);

            return Color.FromArgb(r, g, b);
        }
    }
}