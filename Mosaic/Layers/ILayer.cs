using System.Collections.Generic;

namespace Mosaic.Layers {
    internal interface ILayer : IRectangle {
        string Name { get; }

        IEnumerable<Pixel> GetPixels();
    }
}