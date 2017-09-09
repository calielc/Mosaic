using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;

namespace Mosaic.Tests {
    public sealed class ColorExtensionsTests {
        private static IEnumerable<TestCaseData> CasesInterpolete() {
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
        public Color Should_calc_interpolate_color(Color start, Color end, double percent) {
            var actual = start.Interpolate(end, percent);

            return actual;
        }
    }
}