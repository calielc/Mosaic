using System.Threading.Tasks;
using Mosaic.Layers;
using Mosaic.Queue;

namespace Mosaic.Jobs {
    internal sealed class SpitterBot : IActivity {
        private readonly LayerCollection _layerCollection;
        private readonly ActivityQueue _queue;
        private readonly Broadcast _broadcast;
        private readonly ICreator _creator;

        public SpitterBot(LayerCollection layerCollection, ActivityQueue queue, Broadcast broadcast, ICreator creator) {
            _layerCollection = layerCollection;
            _queue = queue;
            _broadcast = broadcast;
            _creator = creator;
        }

        public async Task Run() => await Task.Run(() => {
            var newBot = NewBot(_layerCollection);
            if (newBot is CalcBot) {
                _queue.Add(newBot);
                return;
            }

            _broadcast.Start(this, $"Spliting: {_layerCollection.Width}x{_layerCollection.Height}...");
            try {
                foreach (var newLayerCollection in _layerCollection.SplitBy(3)) {
                    var bot = NewBot(newLayerCollection);
                    _queue.AddSubtask(this, bot);

                    _broadcast.Step(this);
                }
            }
            finally {
                _broadcast.End(this);
            }

            IActivity NewBot(LayerCollection layerCollection) {
                const int maxSize = 48;
                const int maxArea = maxSize * maxSize;

                if (layerCollection.Width * layerCollection.Height > maxArea) {
                    return new SpitterBot(layerCollection, _queue, _broadcast, _creator);
                }

                return new CalcBot(layerCollection, _creator, _broadcast);
            }
        });
    }
}