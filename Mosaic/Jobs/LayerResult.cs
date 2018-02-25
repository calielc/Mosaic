using System;
using Mosaic.Imaging;
using Mosaic.Pools;
using Mosaic.Savers;

namespace Mosaic.Jobs {
    internal class LayerResult : ILayerResult, IDisposable {
        private readonly IRectangle _rectangle;
        private readonly BidimensionalArrayPool<RGBColor>.Item _colors;

        public LayerResult(IRectangle rectangle) {
            _rectangle = rectangle;

            _colors = PoolsHub.BidimensionalArrayOfRGBColor.Rent(rectangle.Width, rectangle.Height);
        }

        public int Order { get; set; }

        public string Name { get; set; }

        public double Score { get; set; }

        public int Left => _rectangle.Left;
        public int Top => _rectangle.Top;

        public int Width => _rectangle.Width;
        public int Height => _rectangle.Height;

        public void Set(int x, int y, RGBColor color) => _colors.Array[x, y] = color;

        RGBColor[,] ILayerResult.Colors => _colors;

        void IDisposable.Dispose() {
            _colors.Return();
        }

    }
}