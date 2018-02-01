using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Mosaic;

namespace MosaicCmd {
    public sealed class Execute : IBroadcaster {
        private readonly ProgramArgs _programArgs;
        private readonly ConcurrentDictionary<object, LineState> _linePerObject = new ConcurrentDictionary<object, LineState>();

        public Execute(ProgramArgs programArgs) {
            _programArgs = programArgs;
        }

        public async Task Exec() {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try {
                var app = new App {
                    UseParallel = _programArgs.UseParallel,
                    Heatmap = _programArgs.Heatmap,
                    AnimatedGif = _programArgs.AnimatedGif,
                    SearchDirectory = _programArgs.SearchDirectory,
                    SearchPattern = _programArgs.SearchPattern,
                    DestinyDirectory = _programArgs.DestinyDirectory,
                    DestinyFilename = _programArgs.DestinyFileName,
                    Broadcaster = this
                };

                await app.Process();
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

        void IBroadcaster.Start(object sender, string text) {
            lock (this) {
                var top = _linePerObject.Count == 0 ? 0 : (_linePerObject.Values.Max(x => x.Top) + 1);
                var tuple = new LineState(top, text.Length + 1);
                _linePerObject[sender] = tuple;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.SetCursorPosition(0, tuple.Top);
                Console.Write(text);
            }
        }

        void IBroadcaster.Step(object sender) {
            var chars = new[] { "|", "/", "-", "\\" };

            lock (this) {
                var tuple = _linePerObject[sender];

                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(tuple.Left, tuple.Top);

                var index = ((int)tuple.Perc + 1) % chars.Length;
                Console.Write(chars[index]);

                tuple.Perc = index;
            }
        }

        void IBroadcaster.Progress(object sender, double perc) {
            lock (this) {
                var tuple = _linePerObject[sender];

                if (perc - tuple.Perc > 0.015) {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(tuple.Left, tuple.Top);
                    Console.Write($"{perc,4:p} ");

                    tuple.Perc = perc;
                }
            }
        }

        void IBroadcaster.End(object sender, TimeSpan elapsed) {
            lock (this) {
                var tuple = _linePerObject[sender];
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(tuple.Left, tuple.Top);
                Console.Write($"(Y) {elapsed:g}");
            }
        }

        private class LineState {
            public LineState(int top, int left) {
                Top = top;
                Left = left;
                Perc = 0;
            }

            public int Top { get; }
            public int Left { get; }
            public double Perc { get; set; }
        }
    }
}