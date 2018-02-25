using System;
using System.IO;
using System.Threading.Tasks;
using ActivityQueue;
using Mosaic.Imaging;
using Mosaic.Jobs;
using Mosaic.Savers;

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
                    queue
                        .Add(new PaletteReducer(images, _broadcast))
                        .Then(new SplitterBot(images, queue, _broadcast))
                        .Then(bot => {
                            var splitterBot = (SplitterBot)bot;
                            return new PickerBot(splitterBot.Cells, hub, _broadcast);
                        })
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

        private HubSaver NewHub(ISize size, Queue queue) {
            var filename = Path.Combine(DestinyDirectory, DestinyFilename);
            return new HubSaver(size, filename, _broadcast, queue) {
                RenderHeatmap = RenderHeatmap,
                RenderTiles = RenderTiles,
            };
        }

        private Queue NewQueue() {
            var result = new Queue {
                Workers = Math.Min(ParallelBots, Environment.ProcessorCount)
            };
            result.AgentReady += (sender, agent) => _broadcast.Start(agent, $"Agent {agent.Id:00}");
            result.AgentWorked += (sender, agent) => _broadcast.Step(agent);
            result.AgentShutdown += (sender, agent) => _broadcast.End(agent);
            return result.Run();
        }
    }
}