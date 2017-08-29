using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;

namespace Mosaic.Tests
{
    public sealed class ColorExtensionsTests
    {
        private static IEnumerable<TestCaseData> CasesInterpolete()
        {
            yield return new TestCaseData(
                Color.FromArgb(0, 0, 0),
                Color.FromArgb(255, 255, 255),
                0.50d
            ).Returns(Color.FromArgb(127, 127, 127));

            yield return new TestCaseData(
                Color.FromArgb(0, 0, 0),
                Color.FromArgb(255, 255, 255),
                0.25d
            ).Returns(Color.FromArgb(63, 63, 63));

            yield return new TestCaseData(
                Color.FromArgb(10, 20, 100),
                Color.FromArgb(200, 30, 151),
                0.75d
            ).Returns(Color.FromArgb(152, 27, 138));
        }

        [TestCaseSource(nameof(CasesInterpolete))]
        public Color Should_calc_interpolate_color(Color start, Color end, double percent)
        {
            return start.Interpolate(end, percent);
        }

        private static IEnumerable<TestCaseData> CasesGroupBy()
        {
            yield return new TestCaseData(
                new[] {
                    Color.FromArgb(10, 10, 10), Color.FromArgb(11, 11, 11),
                    Color.FromArgb(12, 12, 12), Color.FromArgb(13, 13, 13),
                    Color.FromArgb(14, 14, 14)
                },
                2,
                new[] { Color.FromArgb(10, 10, 10), Color.FromArgb(12, 12, 12), Color.FromArgb(14, 14, 14) }
            );

            yield return new TestCaseData(
                new[] {
                    Color.FromArgb(10, 10, 10), Color.FromArgb(11, 11, 11), Color.FromArgb(12, 12, 12),
                    Color.FromArgb(13, 13, 13), Color.FromArgb(14, 14, 14)
                },
                3,
                new[] { Color.FromArgb(11, 11, 11), Color.FromArgb(14, 14, 14) }
            );

            yield return new TestCaseData(
                new[] {
                    Color.FromArgb(10, 10, 10), Color.FromArgb(11, 11, 11), Color.FromArgb(12, 12, 12),
                    Color.FromArgb(200, 210, 125), Color.FromArgb(200, 209, 125), Color.FromArgb(200, 210, 126)
                },
                4,
                new[] { Color.FromArgb(11, 11, 11), Color.FromArgb(200, 210, 125) }
            );

            yield return new TestCaseData(
                new[] {
                    Color.FromArgb(10, 10, 10),
                    Color.FromArgb(10, 10, 25),
                    Color.FromArgb(10, 25, 10),
                    Color.FromArgb(25, 10, 10),
                },
                16,
                new[] { Color.FromArgb(14, 14, 14) }
            );
        }

        [TestCaseSource(nameof(CasesGroupBy))]
        public void Should_group_color_by_distance(IEnumerable<Color> input, double maxDistance, IEnumerable<Color> expected)
        {
            var mixed = input.GroupByDistance(maxDistance);

            var actual = mixed.Select(mix => (Color)mix).ToArray();

            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}