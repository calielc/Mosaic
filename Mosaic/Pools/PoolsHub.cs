namespace Mosaic.Pools {
    internal static class PoolsHub {
        public static readonly ArrayOfArrayPool<double> ArrayOfArrayOfDouble = new ArrayOfArrayPool<double>();

        public static readonly BidimensionalArrayPool<double> BidimensionalArrayOfDouble = new BidimensionalArrayPool<double>();

        public static readonly BidimensionalArrayPool<RGBColor> BidimensionalArrayOfRGBColor = new BidimensionalArrayPool<RGBColor>();
    }
}