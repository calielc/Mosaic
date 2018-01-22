using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Accord.MachineLearning;

namespace Mosaic {
    [DebuggerDisplay("X: {X}, Y: {Y}, Color: {Color}, Chance: {Chance}")]
    internal sealed class PixelBot {
        public PixelBot(int x, int y) {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public double Chance { get; private set; }

        public SingleColor Color { get; private set; }

        public PixelBot Load(ConcurrentBitmapCollection images) {
            var pixels = GetPixels(images);

            var kmeans = new KMeans(3);
            var clusters = kmeans.Learn(pixels);

            var maxProportion = clusters.Proportions.Max();
            var bestCluster = clusters.First(cluster => cluster.Proportion == maxProportion);

            Color = new SingleColor((byte)bestCluster.Centroid[0], (byte)bestCluster.Centroid[1], (byte)bestCluster.Centroid[2]);
            Chance = maxProportion;

            return this;
        }

        private double[][] GetPixels(ConcurrentBitmapCollection images) {
            var result = new double[images.Count][];

            var idx = 0;
            foreach (var image in images) {
                var color = image[X, Y];
                result[idx] = new double[] { color.R, color.G, color.B };

                idx += 1;
            }

            return result;
        }

        public void Save(Bitmap bmp) {
            bmp.SetPixel(X, Y, Color);
        }

#if DEBUG
        public override string ToString() => $"X: {X}, Y: {Y}, Color: {Color}, Chance: {Chance:p}";
#endif
    }
}