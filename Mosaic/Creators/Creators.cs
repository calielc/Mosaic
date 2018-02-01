using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mosaic.Bots;
using Mosaic.Layers;

namespace Mosaic.Creators {
    internal sealed class Creators : ICreator, IDisposable {
        private readonly List<ICreator> _creators;

        public Creators(ISize size, string filename, bool heatmap, bool animatedGif) {
            _creators = new List<ICreator> {
                new RenderCreator(size)
            };

            if (heatmap) {
                _creators.Add(new HeatmapCreator(size));
            }

            if (animatedGif) {
                _creators.Add(new AnimatedGifCreator(filename));
            }
        }

        public async Task Set(BotResult botResult) {
            var tasks = _creators.Select(creator => creator.Set(botResult));

            await Task.WhenAll(tasks);
        }

        public async Task Flush(string filename) {
            var tasks = _creators.Select(creator => creator.Flush(filename));

            await Task.WhenAll(tasks);
        }

        public void Dispose() {
            foreach (var creator in _creators.OfType<IDisposable>()) {
                creator.Dispose();
            }
        }
    }
}