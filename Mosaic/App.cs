using System.IO;
using System.Threading.Tasks;
using Mosaic.Bots;
using Mosaic.Creators;
using Mosaic.Layers;
using Mosaic.Queue;

namespace Mosaic {
    public sealed class App {
        private Broadcast _broadcast;

        public string SearchDirectory { private get; set; }
        public string SearchPattern { private get; set; }

        public string DestinyDirectory { private get; set; }
        public string DestinyFilename { private get; set; }

        public int ParallelBots { private get; set; }

        public bool RenderHeatmap { private get; set; }
        public bool RenderAnimatedGif { private get; set; }
        public bool RenderTiles { private get; set; }

        public IBroadcaster Broadcaster {
            set => _broadcast = value is null ? null : new Broadcast(value);
        }

        public async Task Process() {
            _broadcast.Start(this, "Mosaic");
            try {
                var discovery = NewImageDiscovery();
                var images = await discovery.Load();

                using (var creators = NewCreatorsHub(images, discovery.FirstFilename)) {
                    using (var queue = NewQueue()) {
                        queue.Enqueue(new SpitterBot(images, queue, _broadcast, creators));

                        await queue.RunAndWait();
                    }

                    var filename = Path.Combine(DestinyDirectory, DestinyFilename);
                    await creators.Flush(filename);
                }
            }
            finally {
                _broadcast.End(this);
            }
        }

        private ImageDiscovery NewImageDiscovery()
            => new ImageDiscovery(_broadcast) {
                SearchDirectory = SearchDirectory,
                SearchPattern = SearchPattern,
            };

        private CreatorsHub NewCreatorsHub(ISize size, string filename)
            => new CreatorsHub(size, filename, _broadcast) {
                RenderHeatmap = RenderHeatmap,
                RenderAnimatedGif = RenderAnimatedGif,
                RenderTiles = RenderTiles,
            };

        private BusQueue NewQueue() {
            var result = new BusQueue {
                Workers = ParallelBots
            };
            result.AgentReady += (sender, agent) => _broadcast.Start(agent, $"Agent {agent.Id:00}");
            result.AgentWorked += (sender, agent) => _broadcast.Step(agent);
            result.AgentShutdown += (sender, agent) => _broadcast.End(agent);
            return result;
        }
    }
}