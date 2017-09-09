using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Mosaic;
using PowerArgs;

namespace MosaicCmd {
    static class Program {
        static async Task Main(string[] args) {
            var programArgs = Args.Parse<ProgramArgs>(args);

            if (programArgs.DestinyDirectory == null) {
                var destinyDirectory = $@"{programArgs.SearchDirectory}\Merged-{DateTime.Now:yyyMMdd}\";
                var path = System.IO.Path.GetDirectoryName(destinyDirectory);
                System.IO.Directory.CreateDirectory(path);

                programArgs.DestinyDirectory = destinyDirectory;
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try {
                var manager = new BotManager {
                    UseParallel = programArgs.UseParallel,
                    Heatmap = programArgs.Heatmap,
                    AnimatedGif = programArgs.AnimatedGif,
                    SearchDirectory = programArgs.SearchDirectory,
                    SearchPattern = programArgs.SearchPattern,
                    DestinyDirectory = programArgs.DestinyDirectory,
                    DestinyFilename = programArgs.DestinyFileName,
                };
                manager.OnText += (_, text) => Console.WriteLine(text);
                manager.OnProgress += (_, percentual) => Console.Write($"{percentual,4:p} ");

                manager.LoadImages();
                manager.LoadBots();
                await manager.Save();
            }
            finally {
                stopWatch.Stop();
                Console.WriteLine($"Processed in {stopWatch.Elapsed}");
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
    }
}