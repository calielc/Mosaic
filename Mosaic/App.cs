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

        public int Parallel { private get; set; }

        public bool Heatmap { private get; set; }
        public bool AnimatedGif { private get; set; }

        public IBroadcaster Broadcaster {
            get => Broadcast.Instance;
            set => Broadcast.Instance = value;
        }

        public async Task Process() {
            Broadcast.Start(this, "Mosaic");
            try {
                var discovery = new ImageDiscovery(SearchDirectory, SearchPattern);
                var images = await discovery.Load();

                var pool = new ArrayOfArrayPool<double>(images.Count, 3);

                using (var creators = new Creators.Creators(images, discovery.FirstFilename, Heatmap, AnimatedGif)) {
                    var queue = new BotQueue(creators);
                    queue.Enqueue(new SpitterBot(images, queue, pool));

                    await queue.WaitAll(Math.Max(1, Parallel));

                    var filename = Path.Combine(DestinyDirectory, DestinyFilename);
                    await creators.Flush(filename);
                }
            }
            finally {
                Broadcast.End(this);
            }
        }
    }
}