using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mosaic
{
    class Program
    {
        static void Main(string[] args)
        {
            const string sourceMedium = @"C:\Temp\Sample\Take-1 (800x600)";
            const string sourceSmall = @"C:\Temp\Sample\Take-1 (267x200)";
            var source = sourceMedium;

            var destiny = $@"{source}\Merged-{DateTime.Now:yyyMMddHHmm}\";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destiny));

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                var manager = new BotManager
                {
                    UseParallel = true,
                    Heatmap = true,
                    SearchDirectory = source,
                    SearchPattern = "*.jpg",
                    DestinyDirectory = destiny,
                    DestinyFilename = "mosaic.jpg",
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
