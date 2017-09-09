using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;

namespace Mosaic.Tests {
    public sealed class SingleColorTests {
        [Test]
        public void Should_create() {
            var color = new SingleColor(100, 255, 62);

            Assert.AreEqual(100, color.R, nameof(SingleColor.R));
            Assert.AreEqual(255, color.G, nameof(SingleColor.G));
            Assert.AreEqual(62, color.B, nameof(SingleColor.B));
        }

        [Test]
        public void Should_convert_from_color() {
            SingleColor color = Color.FromArgb(100, 255, 62);

            Assert.AreEqual(100, color.R, nameof(SingleColor.R));
            Assert.AreEqual(255, color.G, nameof(SingleColor.G));
            Assert.AreEqual(62, color.B, nameof(SingleColor.B));
        }

        [Test]
        public void Should_convert_to_color() {
            Color color = new SingleColor(100, 255, 62);

            Assert.AreEqual(100, color.R, nameof(SingleColor.R));
            Assert.AreEqual(255, color.G, nameof(SingleColor.G));
            Assert.AreEqual(62, color.B, nameof(SingleColor.B));
        }

        private static IEnumerable<TestCaseData> CasesEquals() {
            yield return new TestCaseData(new SingleColor(100, 55, 45), new SingleColor(100, 55, 45)).Returns(true);
            yield return new TestCaseData(new SingleColor(100, 55, 45), new SingleColor(101, 55, 45)).Returns(false);
            yield return new TestCaseData(new SingleColor(100, 55, 45), new SingleColor(100, 56, 45)).Returns(false);
            yield return new TestCaseData(new SingleColor(100, 55, 45), new SingleColor(100, 55, 44)).Returns(false);
        }

        [TestCaseSource(nameof(CasesEquals))]
        public bool Shoud_compare_with_other(SingleColor color1, SingleColor color2) {
            var actual = color1.Equals(color2);
            return actual;
        }

        [TestCaseSource(nameof(CasesEquals))]
        public bool Shoud_compare_with_object(SingleColor color1, object color2) {
            var actual = color1.Equals(color2);
            return actual;
        }

        [TestCaseSource(nameof(CasesEquals))]
        public bool Shoud_compare_with_equals(SingleColor color1, SingleColor color2) {
            var actual = color1 == color2;
            return actual;
        }

        [TestCaseSource(nameof(CasesEquals))]
        public bool Shoud_compare_with_differente(SingleColor color1, SingleColor color2) {
            var actual = color1 != color2;
            return !actual;
        }
    }
}