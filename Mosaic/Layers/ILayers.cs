using System.Collections.Generic;

namespace Mosaic.Layers {
    internal interface ILayers : IRectangle, IReadOnlyCollection<ILayer> {
        IEnumerable<Windows> SplitBy(int tilesX, int tilesY);
    }
}