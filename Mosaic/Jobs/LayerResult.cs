using System;
using Mosaic.Layers;
using Mosaic.Pools;

namespace Mosaic.Jobs {
    internal readonly struct LayerResult : ILayerResult, IDisposable {
        private readonly IRectangle _rectangle;
        private readonly BidimensionalArrayPool<RGBColor>.Item _colors;
        private readonly BidimensionalArrayPool<double>.Item _odds;

        public LayerResult(IRectangle rectangle) {
            _rectangle = rectangle;

            _colors = PoolsHub.BidimensionalArrayOfRGBColor.Rent(rectangle.Width, rectangle.Height);
            _odds = PoolsHub.BidimensionalArrayOfDouble.Rent(rectangle.Width, rectangle.Height);
        }

        public int Left => _rectangle.Left;
        public int Top => _rectangle.Top;

        public int Width => _rectangle.Width;
        public int Height => _rectangle.Height;

        RGBColor[,] ILayerResult.Colors => _colors;
        double[,] ILayerResult.Odds => _odds;

        public void Set(int x, int y, RGBColor color) => _colors.Array[x, y] = color;

        public void Set(int x, int y, double odd) => _odds.Array[x, y] = odd;

        void IDisposable.Dispose() {
            _colors.Return();
            _odds.Return();
        }
    }
}