using System;
using System.IO;
using System.Threading.Tasks;
using Mosaic.Bots;

namespace Mosaic {
    public sealed class App {
        public string SearchDirectory { private get; set; }
        public string SearchPattern { private get; set; }

        public string DestinyDirectory { private get; set; }
        public string DestinyFilename { private get; set; }

        public int ParallelBots { private get; set; }

        public bool RenderHeatmap { private get; set; }
        public bool RenderAnimatedGif { private get; set; }
        public bool RenderTiles { private get; set; }

        public IBroadcaster Broadcaster {
            private get => Broadcast.Instance;
            set => Broadcast.Instance = value;
        }

        public async Task Process() {
            Broadcast.Start(this, "Mosaic");
            try {
                var discovery = new ImageDiscovery(SearchDirectory, SearchPattern);
                var images = await discovery.Load();

                using (var creators = new Creators.Creators(images, discovery.FirstFilename, RenderHeatmap, RenderAnimatedGif, RenderTiles)) {
                    var queue = new BotQueue(creators);
                    queue.Enqueue(new SpitterBot(images, queue));

                    var consumers = GetConsumers();
                    await queue.WaitAll(consumers);

                    var filename = Path.Combine(DestinyDirectory, DestinyFilename);
                    await creators.Flush(filename);
                }
            }
            finally {
                Broadcast.End(this);
            }
        }

        private int GetConsumers() {
            if (ParallelBots <= 1) {
                return 1;
            }

            if (ParallelBots >= Environment.ProcessorCount) {
                return Environment.ProcessorCount;
            }

            return ParallelBots;
        }
    }
}