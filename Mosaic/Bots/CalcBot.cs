using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Accord.MachineLearning;
using Mosaic.Creators;
using Mosaic.Layers;

namespace Mosaic.Bots {
    internal sealed class CalcBot : IBot {
        private readonly LayerCollection _layerCollection;

        public CalcBot(LayerCollection layerCollection) {
            _layerCollection = layerCollection;
        }

        public async Task Process(ICreator creator) => await Task.Run(async () => {
            var done = 0;
            double total = _layerCollection.Width * _layerCollection.Height;

            Broadcast.Start(this, $"Layer: {_layerCollection.Left:000}+{_layerCollection.Width:00} x {_layerCollection.Top:000}+{_layerCollection.Height:00}...");
            try {
                using (var result = new LayerResult(_layerCollection)) {
                    for (var x = 0; x < _layerCollection.Width; x++) {
                        for (var y = 0; y < _layerCollection.Height; y++) {
                            var group = new Group(x, y, _layerCollection.Select(layer => layer[x, y]), _layerCollection.Count, result);
                            group.Process();

                            Broadcast.Progress(this, ++done / total);
                        }
                    }

                    await creator.Set(result);
                }
            }
            finally {
                Broadcast.End(this);
            }
        });

        [DebuggerDisplay("x: {_x}, y: {_y}")]
        private readonly struct Group {
            private readonly int _x;
            private readonly int _y;
            private readonly IEnumerable<RGBColor> _colors;
            private readonly int _counts;
            private readonly LayerResult _layerResult;

            public Group(int x, int y, IEnumerable<RGBColor> colors, int counts, LayerResult layerResult) {
                _x = x;
                _y = y;
                _colors = colors;
                _counts = counts;
                _layerResult = layerResult;
            }

            public void Process() {
                var kmeans = new KMeans(3);

                var rentedArray = PoolsHub.ArrayOfArrayOfDouble.Rent(_counts, 3);
                Fill(rentedArray);

                try {
                    var clusters = kmeans.Learn(rentedArray);

                    var maxProportion = clusters.Proportions.Max();
                    var bestCluster = clusters.First(cluster => cluster.Proportion >= maxProportion);

                    Flush(bestCluster);
                }
                finally {
                    rentedArray.Return();
                }
            }

            private void Fill(double[][] array) {
                var tuples = _colors.Select((item, index) => (row: array[index], color: item));
                Parallel.ForEach(tuples, tuple => {
                    tuple.row[0] = tuple.color.R;
                    tuple.row[1] = tuple.color.G;
                    tuple.row[2] = tuple.color.B;
                });
            }

            private void Flush(KMeansClusterCollection.KMeansCluster cluster) {
                _layerResult.Set(_x, _y, new RGBColor((byte)cluster.Centroid[0], (byte)cluster.Centroid[1], (byte)cluster.Centroid[2]));
                _layerResult.Set(_x, _y, cluster.Proportion);
            }
        }
    }
}