using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Accord.MachineLearning;
using Mosaic.Layers;
using Mosaic.Pools;
using Mosaic.Queue;

namespace Mosaic.Jobs {
    internal sealed class CalcBot : IActivity {
        private readonly LayerCollection _layerCollection;
        private readonly Broadcast _broadcast;
        private readonly ICreator _creator;

        public CalcBot(LayerCollection layerCollection, ICreator creator, Broadcast broadcast) {
            _layerCollection = layerCollection;
            _broadcast = broadcast;
            _creator = creator;
        }

        public async Task Run() => await Task.Run(async () => {
            var done = 0;
            double total = _layerCollection.Width * _layerCollection.Height;

            _broadcast.Start(this, $"Layer: {_layerCollection.Left:000}+{_layerCollection.Width:00} x {_layerCollection.Top:000}+{_layerCollection.Height:00}...");
            try {
                using (var result = new LayerResult(_layerCollection)) {
                    for (var x = 0; x < _layerCollection.Width; x++) {
                        for (var y = 0; y < _layerCollection.Height; y++) {
                            var group = new Group(x, y, _layerCollection.Select(layer => layer[x, y]), _layerCollection.Count, result);
                            group.Process();

                            _broadcast.Progress(this, ++done / total);
                        }
                    }

                    await _creator.Set(result);
                }
            }
            finally {
                _broadcast.End(this);
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
                var rentedArray = PoolsHub.ArrayOfArrayOfDouble.Rent(_counts, 3);
                try {
                    Fill(rentedArray);

                    const int segments = 12;
                    var kmeans = new KMeans(segments);
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
                Parallel.ForEach(tuples, tuple => tuple.color.CopyTo(tuple.row));
            }

            private void Flush(KMeansClusterCollection.KMeansCluster cluster) {
                _layerResult.Set(_x, _y, RGBColor.CopyFrom(cluster.Centroid));
                _layerResult.Set(_x, _y, cluster.Proportion);
            }
        }
    }
}