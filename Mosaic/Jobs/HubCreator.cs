using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mosaic.Layers;
using Mosaic.Queue;

namespace Mosaic.Jobs {
    internal sealed class HubCreator : ICreator, IActivity, IDisposable {
        private readonly ActivityQueue _queue;
        private readonly RenderCreator _renderCreator;
        private readonly HeatmapCreator _heatmapCreator;
        private readonly TilesCreator _tilesCreator;
        private readonly IReadOnlyCollection<ICreator> _creators;

        public HubCreator(ISize size, string filename, Broadcast broadcast, ActivityQueue queue) {
            _queue = queue;

            _renderCreator = new RenderCreator(size, filename, broadcast);
            _heatmapCreator = new HeatmapCreator(size, filename, broadcast);
            _tilesCreator = new TilesCreator(size, filename, broadcast);
            _creators = new ICreator[] { _renderCreator, _heatmapCreator, _tilesCreator, };
        }

        public bool RenderHeatmap { get; set; }
        public bool RenderTiles { get; set; }

        public async Task Set(ILayerResult input) {
            var tasks = _creators.Select(creator => creator.Set(input));

            await Task.WhenAll(tasks);
        }

        public async Task Run() => await Task.Factory.StartNew(() => {
            _queue.AddSubtask(this, _renderCreator);
            if (RenderHeatmap) {
                _queue.AddSubtask(this, _heatmapCreator);
            }
            if (RenderHeatmap) {
                _queue.AddSubtask(this, _tilesCreator);
            }
        });

        public void Dispose() {
            foreach (var creator in _creators.OfType<IDisposable>()) {
                creator.Dispose();
            }
        }
    }
}