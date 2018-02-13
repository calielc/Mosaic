using System.Threading.Tasks;
using Mosaic.Creators;
using Mosaic.Layers;

namespace Mosaic.Bots {
    internal sealed class SpitterBot : IBot {
        private readonly LayerCollection _layerCollection;
        private readonly BotQueue _queue;

        public SpitterBot(LayerCollection layerCollection, BotQueue queue) {
            _layerCollection = layerCollection;
            _queue = queue;
        }

        public async Task Process(ICreator creator) => await Task.Run(() => {
            var newBot = NewBot(_layerCollection);
            if (newBot is CalcBot) {
                _queue.Enqueue(newBot);
                return;
            }

            Broadcast.Start(this, $"Spliting: {_layerCollection.Width}x{_layerCollection.Height}...");
            try {
                foreach (var newLayerCollection in _layerCollection.SplitBy(3)) {
                    var bot = NewBot(newLayerCollection);
                    _queue.Enqueue(bot);

                    Broadcast.Step(this);
                }
            }
            finally {
                Broadcast.End(this);
            }

            IBot NewBot(LayerCollection layerCollection) {
                const int maxArea = 50 * 50;
                if (layerCollection.Width * layerCollection.Height > maxArea) {
                    return new SpitterBot(layerCollection, _queue);
                }

                return new CalcBot(layerCollection);
            }
        });
    }
}