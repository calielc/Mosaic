using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.MachineLearning;
using Mosaic.Creators;

namespace Mosaic.Bots {
    internal sealed class CalcBot : IBot {
        private readonly Layers.LayerCollection _layerCollection;
        private readonly ArrayOfArrayPool<double> _pool;

        public CalcBot(Layers.LayerCollection layerCollection, ArrayOfArrayPool<double> pool) {
            _layerCollection = layerCollection;

            _pool = pool.ExternalCount == _layerCollection.Count && pool.InternalCount == 3
                ? pool
                : new ArrayOfArrayPool<double>(_layerCollection.Count, 3);
        }

        public async Task Process(ICreator creator) => await Task.Run(async () => {
            Broadcast.Start(this, $"Calcing: {_layerCollection.Left}x{_layerCollection.Top}...");
            try {
                var colors = new RGBColor[_layerCollection.Width, _layerCollection.Height];
                var odds = new double[_layerCollection.Width, _layerCollection.Height];

                var groups = from layer in _layerCollection
                             let pixels = layer.GetPixels()
                             from pixel in pixels
                             group pixel by (pixel.X, pixel.Y) into g
                             select (x: g.Key.X, y: g.Key.Y, colors: g.Select(k => k.Color));

                var done = 0;
                double total = _layerCollection.Width * _layerCollection.Height;

                Parallel.ForEach(groups, group => {
                    ProcessGroup(in group, in colors, in odds);

                    done += 1;
                    Broadcast.Progress(this, done / total);
                });

                var result = new BotResult(_layerCollection, colors, odds);
                await creator.Set(result);
            }
            finally {
                Broadcast.End(this);
            }
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