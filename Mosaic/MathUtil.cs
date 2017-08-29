using System.Runtime.CompilerServices;

namespace Mosaic
{
    internal static class MathUtils
    {
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
    }
}