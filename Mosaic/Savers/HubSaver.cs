using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivityQueue;
using Mosaic.Imaging;

namespace Mosaic.Savers {
    internal sealed class HubSaver : ISaver, IDisposable {
        private readonly Queue _queue;
        private readonly ISaver _puzzleSaver;
        private readonly ISaver _heatmapSaver;
        private readonly ISaver _tileSaver;
        private readonly IReadOnlyCollection<ISaver> _creators;

        public HubSaver(ISize size, string filename, Broadcast broadcast, Queue queue) {
            _queue = queue;

            _puzzleSaver = new PuzzleSaver(size, filename, broadcast);
            _heatmapSaver = new HeatmapSaver(size, filename, broadcast);
            _tileSaver = new TileSaver(size, filename, broadcast);
            _creators = new[] { _puzzleSaver, _heatmapSaver, _tileSaver, };
        }

        public bool RenderHeatmap { private get; set; }
        public bool RenderTiles { private get; set; }

        public async Task Set(ILayerResult input) {
            var tasks = _creators.Select(creator => creator.Set(input));

            await Task.WhenAll(tasks);
        }

        public async Task Run() => await Task.Factory.StartNew(() => {
            _queue.AddSubtask(this, _puzzleSaver);

            if (RenderHeatmap) {
                _queue.AddSubtask(this, _heatmapSaver);
            }

            if (RenderTiles) {
                _queue.AddSubtask(this, _tileSaver);
            }
        });

        public void Dispose() {
            foreach (var creator in _creators.OfType<IDisposable>()) {
                creator.Dispose();
            }
        }
    }
}