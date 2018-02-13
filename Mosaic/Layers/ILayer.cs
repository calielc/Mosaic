namespace Mosaic.Layers {
    internal interface ILayer : IRectangle {
        string Name { get; }

        RGBColor this[int x, int y] { get; }
    }
}