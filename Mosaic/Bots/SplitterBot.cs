using System.Threading.Tasks;
using Mosaic.Creators;
using Mosaic.Layers;

namespace Mosaic.Bots {
    internal sealed class SplitterBot : IBot {
        private readonly ILayers _layers;
        private readonly DoubleBidimensionalArrayPool _pool;

        public SplitterBot(ILayers layers, DoubleBidimensionalArrayPool pool) {
            _layers = layers;
            _pool = pool;
        }

        public async Task Process(ICreator creator) {
            Broadcast.Start(this, $"Splitting {_layers.Left}x{_layers.Top} - {_layers.Width}x{_layers.Height}...");

            var splits = _layers.SplitBy(3, 3);
            foreach (var windows in splits) {
                await new Bot(windows, _pool).Process(creator);

                Broadcast.Step(this);
            }

            Broadcast.End(this);
        }
    }
}