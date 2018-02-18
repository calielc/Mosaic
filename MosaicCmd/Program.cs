using System;
using System.IO;
using System.Threading.Tasks;
using Mosaic;
using PowerArgs;

namespace MosaicCmd {
    static class Program {
        static async Task Main(string[] args) {
            var programArgs = Args.Parse<ProgramArgs>(args);

            if (programArgs.DestinyDirectory == null) {
                var destinyDirectory = $@"{programArgs.SearchDirectory}\Merged-{DateTime.Now:yyyMMdd}\";
                var path = Path.GetDirectoryName(destinyDirectory);
                Directory.CreateDirectory(path);

                programArgs.DestinyDirectory = destinyDirectory;
            }

            var broadcaster = new Broadcaster();

            await new App {
                ParallelBots = programArgs.Parallel,

                RenderHeatmap = programArgs.Heatmap,
                RenderTiles = programArgs.Tiles,

                SearchDirectory = programArgs.SearchDirectory,
                SearchPattern = programArgs.SearchPattern,

                DestinyDirectory = programArgs.DestinyDirectory,
                DestinyFilename = programArgs.DestinyFileName,

                Broadcaster = broadcaster
            }.Process();

            broadcaster.Finish();
        }
    }
}