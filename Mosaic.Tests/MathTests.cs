using NUnit.Framework;

namespace Mosaic.Tests
{
    public sealed class MathTests
    {
        [TestCase(10, 30, ExpectedResult = 400d)]
        [TestCase(50, 40, ExpectedResult = 100d)]
        public double Should_calc_sqr_differente_between_2_integers(int value1, int value2)
        {
            var actual = MathUtils.DifferenceSqr(value1, value2);

            return actual;
        }
    }
}
