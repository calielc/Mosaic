using System;
using System.Collections.Concurrent;
using System.Linq;
using Mosaic;

namespace MosaicCmd {
    public sealed class Broadcaster : IBroadcaster {
        private static readonly TimeSpan RefreshRate = TimeSpan.FromSeconds(1);

        private readonly ConcurrentDictionary<object, LineState> _lineStates = new ConcurrentDictionary<object, LineState>();
        private readonly string[] _animationChars = { "|", "/", "-", "\\" };

        public void Start(object sender, string text) {
            lock (this) {
                if (_lineStates.TryGetValue(sender, out var lineState) == false) {
                    var top = _lineStates.Count == 0
                        ? 0
                        : _lineStates.Values.Max(item => item.Top) + 1;
                    lineState = _lineStates[sender] = new LineState(top, text.Length + 1);
                }
                else {
                    lineState.Perc = lineState.Step = 0;
                }

                WriteText(0, lineState.Top, ConsoleColor.Yellow, text);
            }
        }

        public void Step(object sender) {
            var lineState = _lineStates[sender];
            lineState.Step += 1;

            if (DateTime.Now - lineState.LastRefresh < RefreshRate) {
                return;
            }

            lock (this) {
                WriteText(lineState.Left, lineState.Top, ConsoleColor.White, $"{_animationChars[lineState.Step % _animationChars.Length]} ({lineState.Step})");
                lineState.LastRefresh = DateTime.Now;
            }
        }

        public void Progress(object sender, double perc) {
            var lineState = _lineStates[sender];
            lineState.Perc = perc;

            if (DateTime.Now - lineState.LastRefresh < RefreshRate) {
                return;
            }

            lock (this) {
                WriteText(lineState.Left, lineState.Top, ConsoleColor.White, $"{lineState.Perc,4:p}");
                lineState.LastRefresh = DateTime.Now;
            }

        }

        public void End(object sender, TimeSpan elapsed) {
            var lineState = _lineStates[sender];

            lock (this) {
                WriteText(lineState.Left, lineState.Top, ConsoleColor.Green, $"(Y) {elapsed:g}");
            }
        }

        public void Finish() {
            var top = _lineStates.Values.Max(item => item.Top) + 5;
            WriteText(0, top, ConsoleColor.White, "done, press something....");

            Console.ReadKey();
        }

        private static void WriteText(int left, int top, ConsoleColor color, string text) {
            Console.ForegroundColor = color;
            Console.SetCursorPosition(left, top);
            Console.Write(text);
        }

        private sealed class LineState {
            public LineState(int top, int left) {
                Top = top;
                Left = left;
                Perc = 0;
                Step = 0;
            }

            public int Top { get; }
            public int Left { get; }
            public double Perc { get; set; }
            public int Step { get; set; }

            public DateTime? LastRefresh { get; set; }
        }
    }
}