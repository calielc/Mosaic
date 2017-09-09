using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;

namespace Mosaic.Tests {
    public sealed class MixedColorTests {
        [Test]
        public void Should_convert_from_color_to_mix() {
            var mixed = (MixedColor) new SingleColor(128, 0, 128);

            Assert.AreEqual(new SingleColor(128, 0, 128), mixed.Current);
            Assert.AreEqual(1, mixed.Count);
        }

        [Test]
        public void Should_convert_from_mix_to_color() {
            var mixed = new MixedColor(Color.Blue).Add(Color.Green);

            var actual = (SingleColor) mixed;
            Assert.AreEqual(mixed.Current, actual);
        }

        [Test]
        public void Should_create_with_one_color_1_time() {
            var mixed = new MixedColor(Color.FromArgb(255, 0, 0));

            Assert.AreEqual(new SingleColor(255, 0, 0), mixed.Current);
            Assert.AreEqual(1, mixed.Count);
        }

        [Test]
        public void Should_create_with_one_color_multiple_times() {
            var mixed = new MixedColor(Color.FromArgb(200, 255, 69), 20);

            Assert.AreEqual(new SingleColor(200, 255, 69), mixed.Current);
            Assert.AreEqual(20, mixed.Count);
        }

        [Test]
        public void Should_add_color_and_create_a_mix() {
            var mixed = new MixedColor(Color.Black);
            mixed.Add(Color.White);

            Assert.AreEqual(new SingleColor(128, 128, 128), mixed.Current);
            Assert.AreEqual(127.5d, mixed.R, nameof(MixedColor.R));
            Assert.AreEqual(127.5d, mixed.G, nameof(MixedColor.G));
            Assert.AreEqual(127.5d, mixed.B, nameof(MixedColor.B));
            Assert.AreEqual(2, mixed.Count);
        }

        [Test]
        public void Should_add_color_and_create_an_average_mix() {
            var mix = new MixedColor(Color.Black);
            mix.Add(Color.White, 2);
            mix.Add(Color.FromArgb(200, 157, 69));

            Assert.AreEqual(new SingleColor(178, 167, 145), mix.Current, nameof(MixedColor.Current));
            Assert.AreEqual(177.5d, mix.R, nameof(MixedColor.R));
            Assert.AreEqual(166.75d, mix.G, nameof(MixedColor.G));
            Assert.AreEqual(144.75d, mix.B, nameof(MixedColor.B));
            Assert.AreEqual(4, mix.Count, nameof(MixedColor.Count));
        }

        [Test]
        public void Should_add_another_mix() {
            var mix = new MixedColor(Color.FromArgb(58, 57, 59)).Add(Color.FromArgb(14, 32, 47));
            var another = new MixedColor(Color.FromArgb(123, 124, 125)).Add(Color.FromArgb(64, 87, 10));
            mix.Add(another);

            Assert.AreEqual(new SingleColor(65, 75, 60), mix.Current, nameof(MixedColor.Current));
            Assert.AreEqual(64.75d, mix.R, nameof(MixedColor.R));
            Assert.AreEqual(75.0d, mix.G, nameof(MixedColor.G));
            Assert.AreEqual(60.25d, mix.B, nameof(MixedColor.B));
            Assert.AreEqual(4, mix.Count, nameof(MixedColor.Current));
        }

        private static IEnumerable<TestCaseData> CasesDistance() {
            yield return new TestCaseData(
                new MixedColor(Color.Black),
                new MixedColor(Color.Black)
            ).Returns(0d);

            yield return new TestCaseData(
                new MixedColor(Color.Black),
                new MixedColor(Color.White)
            ).Returns(441.672955930063709849498817084d);

            yield return new TestCaseData(
                new MixedColor(new SingleColor(100, 253, 12)),
                new MixedColor(new SingleColor(69, 14, 218))
            ).Returns(317.04573802528871086631746663429d);

            var mix1 = new MixedColor(new SingleColor(100, 253, 12), 7);
            mix1.Add(new SingleColor(240, 255, 255));
            mix1.Add(new SingleColor(255, 10, 9));
            var mix2 = new MixedColor(new SingleColor(69, 14, 218), 3);
            mix2.Add(new SingleColor(0, 0, 0));
            mix2.Add(new SingleColor(30, 25, 47));
            yield return new TestCaseData(
                mix1,
                mix2
            ).Returns(250.78213849406086d);
        }

        [TestCaseSource(nameof(CasesDistance))]
        public double Should_calc_distance(MixedColor mix1, MixedColor mix2) {
            var actual = mix1.Distance(mix2);

            return actual;
        }

        private static IEnumerable<TestCaseData> CasesEquals() {
            yield return new TestCaseData(
                new MixedColor(Color.FromArgb(100, 100, 100), 10),
                new MixedColor(Color.FromArgb(100, 100, 100), 10)
            ).Returns(true);

            yield return new TestCaseData(
                new MixedColor(Color.FromArgb(100, 100, 100), 10),
                new MixedColor(Color.FromArgb(100, 100, 100), 09)
            ).Returns(false);

            yield return new TestCaseData(
                new MixedColor(Color.FromArgb(100, 100, 100), 10),
                new MixedColor(Color.FromArgb(100, 100, 100), 11)
            ).Returns(false);

            yield return new TestCaseData(
                new MixedColor(Color.FromArgb(100, 100, 100), 10).Add(Color.FromArgb(200, 254, 243)),
                new MixedColor(Color.FromArgb(100, 100, 100), 10).Add(Color.FromArgb(200, 254, 243))
            ).Returns(true);

            yield return new TestCaseData(
                new MixedColor(Color.FromArgb(100, 100, 100), 10).Add(Color.FromArgb(200, 254, 243)),
                new MixedColor(Color.FromArgb(100, 100, 100), 10).Add(Color.FromArgb(200, 254, 242))
            ).Returns(false);

            yield return new TestCaseData(
                new MixedColor(Color.Green).Add(Color.Brown),
                new MixedColor(Color.Brown).Add(Color.Green)
            ).Returns(true);

            var mix = new MixedColor(Color.GreenYellow);
            yield return new TestCaseData(mix, mix).Returns(true);
            yield return new TestCaseData(mix, null).Returns(false);
        }

        [TestCaseSource(nameof(CasesEquals))]
        public bool Should_compare_using_equals(MixedColor mix1, MixedColor mix2) {
            var actual = mix1.Equals(mix2);

            return actual;
        }

        [TestCaseSource(nameof(CasesEquals))]
        public bool Should_compare_using_object(MixedColor mix1, object mix2) {
            var actual = mix1.Equals(mix2);

            return actual;
        }

        [TestCaseSource(nameof(CasesEquals))]
        public bool Should_compare_using_operator_equals(MixedColor mix1, MixedColor mix2) {
            var actual = mix1 == mix2;

            return actual;
        }

        [TestCaseSource(nameof(CasesEquals))]
        public bool Should_compare_using_operator_different(MixedColor mix1, MixedColor mix2) {
            var actual = mix1 != mix2;

            return !actual;
        }
    }
}