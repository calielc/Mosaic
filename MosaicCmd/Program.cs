using System;
using System.Diagnostics;
using Mosaic;
using PowerArgs;

namespace MosaicCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            var programArgs = Args.Parse<ProgramArgs>(args);

            if (programArgs.DestinyDirectory == null)
            {
                var destinyDirectory = $@"{programArgs.SearchDirectory}\Merged-{DateTime.Now:yyyMMddHHmm}\";
                var path = System.IO.Path.GetDirectoryName(destinyDirectory);
                System.IO.Directory.CreateDirectory(path);

                programArgs.DestinyDirectory = destinyDirectory;
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                var manager = new BotManager
                {
                    UseParallel = programArgs.UseParallel,
                    Heatmap = programArgs.Heatmap,
                    AnimatedGif = programArgs.AnimatedGif,
                    SearchDirectory = programArgs.SearchDirectory,
                    SearchPattern = programArgs.SearchPattern,
                    DestinyDirectory = programArgs.DestinyDirectory,
                    DestinyFilename = programArgs.DestinyFileName,
                };
                manager.OnText += (_, text) => Console.WriteLine(text);
                manager.OnProgress += (_, percentual) => Console.Write($"{percentual:p} ");

                manager.LoadImages();
                manager.CreateBots();
                manager.LoadBots();
                manager.SaveToFile();
            }
            finally
            {
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
