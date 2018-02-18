using System;
using System.IO;
using System.Threading.Tasks;
using Mosaic.Jobs;
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
        public bool RenderTiles { private get; set; }

        public IBroadcaster Broadcaster {
            set => _broadcast = value is null ? null : new Broadcast(value);
        }

        public async Task Process() {
            _broadcast.Start(this, "Mosaic");
            try {
                var discovery = NewImageDiscovery();
                var images = await discovery.Load();

                var queue = NewQueue();
                var hub = NewHub(images, queue);
                try {
                    queue.Run()
                        .Add(new PaletteReducer(images, _broadcast))
                        .Then(new SpitterBot(images, queue, _broadcast, hub))
                        .Then(hub);

                    await queue.Wait();
                }
                finally {
                    hub.Dispose();
                    queue.Dispose();
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

        private HubCreator NewHub(ISize size, ActivityQueue queue) {
            var filename = Path.Combine(DestinyDirectory, DestinyFilename);
            return new HubCreator(size, filename, _broadcast, queue) {
                RenderHeatmap = RenderHeatmap,
                RenderTiles = RenderTiles,
            };
        }

        private ActivityQueue NewQueue() {
            var result = new ActivityQueue {
                Workers = Math.Min(ParallelBots, Environment.ProcessorCount)
            };
            result.AgentReady += (sender, agent) => _broadcast.Start(agent, $"Agent {agent.Id:00}");
            result.AgentWorked += (sender, agent) => _broadcast.Step(agent);
            result.AgentShutdown += (sender, agent) => _broadcast.End(agent);
            return result;
        }
    }
}