using System.IO;
using System.Threading.Tasks;
using Mosaic.Bots;

namespace Mosaic {
    public sealed class App {
        public string SearchDirectory { private get; set; }
        public string SearchPattern { private get; set; }

        public string DestinyDirectory { private get; set; }
        public string DestinyFilename { private get; set; }

        public bool UseParallel { private get; set; }
        public bool Heatmap { private get; set; }
        public bool AnimatedGif { private get; set; }

        public IBroadcaster Broadcaster {
            get => Broadcast.Instance;
            set => Broadcast.Instance = value;
        }

        public async Task Process() {
            var discovery = new ImageDiscovery(SearchDirectory, SearchPattern);
            var images = discovery.Load();

            using (var creators = new Creators.Creators(images, discovery.FirstFilename, Heatmap, AnimatedGif)) {
                await Bot.Start(images, creators);

                var filename = Path.Combine(DestinyDirectory, DestinyFilename);
                await creators.Flush(filename);
            }
        }
    }
}