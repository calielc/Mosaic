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
            var robot = new MosaicManager();

            Console.WriteLine("Loading images...");
            robot.LoadImages(@"C:\Temp\Sample\Take-1a");

            robot.CreateBots();

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                robot.Process();
            }
            finally
            {
                stopWatch.Stop();
                Console.WriteLine($"Processed in {stopWatch.Elapsed}");
            }

            Console.WriteLine("Saving image...");
            robot.SaveToFile(@"C:\Temp\Sample\Take-1a.jpg");
        }
    }
}
