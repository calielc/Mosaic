using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Mosaic
{
    internal static class Extensions
    {
        public static IEnumerable<MixedColor> GroupByDistance(this IEnumerable<MixedColor> self, decimal maxDistance)
        {
            decimal ColorDistance(Color c1, Color c2)
            {
                var r = Math.Pow(c1.R - c2.R, 2);
                var g = Math.Pow(c1.G - c2.G, 2);
                var b = Math.Pow(c1.B - c2.B, 2);
                return (decimal)Math.Sqrt(r + g + b);
            }

            var resultCollection = new List<MixedColor>();
            foreach (var sourceItem in self)
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
    }
}