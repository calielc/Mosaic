using System.Threading.Tasks;
using Mosaic.Creators;
using Mosaic.Layers;

namespace Mosaic.Bots {
    internal sealed class Bot : IBot {
        private readonly IBot _internal;

        public Bot(ILayers layers, DoubleBidimensionalArrayPool pool) {
            if (layers.Width > 100 && layers.Height > 100) {
                _internal = new SplitterBot(layers, pool);
            }
            else {
                _internal = new CalcBot(layers, pool);
            }
        }

        public async Task Process(ICreator creator) => await _internal.Process(creator);

        public static async Task Start(ILayers layers, Creators.Creators creators) {
            var pool = new DoubleBidimensionalArrayPool(layers.Count);

            await new Bot(layers, pool).Process(creators);
        }
    }
}