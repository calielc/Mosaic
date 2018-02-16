using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mosaic.Layers;

namespace Mosaic.Creators {
    internal sealed class CreatorsHub : ICreator, IDisposable {
        private readonly IReadOnlyCollection<ICreator> _allCreators;
        private readonly List<ICreator> _usedCreators;

        public CreatorsHub(ISize size, string filename, Broadcast broadcast) {
            _allCreators = new List<ICreator> {
                new RenderCreator(size)
            };

            _allCreators = new ICreator[] {
                new HeatmapCreator(size, broadcast),
                new AnimatedGifCreator(filename, broadcast),
                new TilesCreator(size, broadcast),
            };
            _usedCreators = new List<ICreator>();
        }

        public bool RenderHeatmap { get; set; }
        public bool RenderAnimatedGif { get; set; }
        public bool RenderTiles { get; set; }

        public async Task Set(ILayerResult input) {
            var tasks = _usedCreators.Select(creator => creator.Set(input));

            await Task.WhenAll(tasks);
        }

        public async Task Flush(string filename) {
            var tasks = _usedCreators.Select(creator => creator.Flush(filename));

            await Task.WhenAll(tasks);
        }

        public void Dispose() {
            foreach (var creator in _allCreators.OfType<IDisposable>()) {
                creator.Dispose();
            }
        }
    }
}