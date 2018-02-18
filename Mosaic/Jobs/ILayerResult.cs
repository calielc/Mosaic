using Mosaic.Layers;

namespace Mosaic.Jobs {
    internal interface ILayerResult : IRectangle {
        RGBColor[,] Colors { get; }
        double[,] Odds { get; }
    }
}