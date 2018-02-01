using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.MachineLearning;
using Mosaic.Creators;
using Mosaic.Layers;

namespace Mosaic.Bots {
    internal sealed class CalcBot : IBot {
        private readonly ILayers _layers;
        private readonly DoubleBidimensionalArrayPool _pool;

        public CalcBot(ILayers layers, DoubleBidimensionalArrayPool sharedPool) {
            _layers = layers;

            _pool = sharedPool.ExternalCount == _layers.Count
                ? sharedPool
                : new DoubleBidimensionalArrayPool(_layers.Count);
        }

        public async Task Process(ICreator creator) => await Task.Factory.StartNew(async () => {
            Broadcast.Start(this, $"Calcing {_layers.Left}x{_layers.Top}...");

            var colors = new RGBColor[_layers.Width, _layers.Height];
            var odds = new double[_layers.Width, _layers.Height];

            var groups = from layer in _layers
                         let pixels = layer.GetPixels()
                         from pixel in pixels
                         group pixel by (pixel.X, pixel.Y) into g
                         select (x: g.Key.X, y: g.Key.Y, colors: g.Select(k => k.Color));

            Parallel.ForEach(groups, group => {
                ProcessGroup(in group, in colors, in odds);
                Broadcast.Step(this);
            });

            var result = new BotResult(_layers, colors, odds);
            await creator.Set(result);

            Broadcast.End(this);
        });

        private void ProcessGroup(in (int x, int y, IEnumerable<RGBColor> colors) group, in RGBColor[,] colors, in double[,] odds) {
            var kmeans = new KMeans(3);

            var rentedArray = _pool.Rent().Fill(group.colors, (row, color) => {
                row[0] = color.R;
                row[1] = color.G;
                row[2] = color.B;
            });
            try {
                var clusters = kmeans.Learn(rentedArray);

                var maxProportion = clusters.Proportions.Max();
                var bestCluster = clusters.First(cluster => cluster.Proportion >= maxProportion);

                colors[group.x, group.y] = new RGBColor((byte)bestCluster.Centroid[0], (byte)bestCluster.Centroid[1], (byte)bestCluster.Centroid[2]);
                odds[group.x, group.y] = maxProportion;
            }
            finally {
                rentedArray.Return();
            }
        }
    }
}