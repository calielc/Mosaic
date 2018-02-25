using Mosaic.Imaging;

namespace Mosaic.Savers {
    internal interface ILayerResult : IRectangle {
        int Order { get; }
        string Name { get; }
        double Score { get; }
        RGBColor[,] Colors { get; }
    }
}