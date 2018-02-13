using Mosaic.Layers;

namespace Mosaic.Creators {
    internal interface ILayerResult : IRectangle {
        RGBColor[,] Colors { get; }
        double[,] Odds { get; }
    }
}