using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mosaic.Layers;

namespace Mosaic.Creators {
    internal sealed class Creators : ICreator, IDisposable {
        private readonly List<ICreator> _creators;

        public Creators(ISize size, string filename, bool heatmap, bool animatedGif, bool tiles) {
            _creators = new List<ICreator> {
                new RenderCreator(size)
            };

            if (heatmap) {
                _creators.Add(new HeatmapCreator(size));
            }

            if (animatedGif) {
                _creators.Add(new AnimatedGifCreator(filename));
            }

            if (tiles) {
                _creators.Add(new TilesCreator(size));
            }
        }

        public async Task Set(ILayerResult input) {
            var tasks = _creators.Select(creator => creator.Set(input));

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