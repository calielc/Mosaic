using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivityQueue;
using Mosaic.Imaging;

namespace Mosaic.Jobs {
    internal sealed class SplitterBot : IActivity {
        private const int PuzzleSize = 96;
        private readonly ImageCollection _images;
        private readonly Broadcast _broadcast;
        private readonly Queue _queue;
        private readonly ConcurrentBag<Cell> _bots = new ConcurrentBag<Cell>();

        public SplitterBot(ImageCollection images, Queue queue, Broadcast broadcast) {
            _images = images;
            _broadcast = broadcast;
            _queue = queue;
        }

        public IReadOnlyCollection<Cell> Cells => _bots;

        public async Task Run() => await Task.Run(() => {
            _broadcast.Start(this, $"Spliting: {_images.Width}x{_images.Height} by {PuzzleSize}x{PuzzleSize}");

            try {
                var bots =
                    from h in Split(_images.Width, PuzzleSize)
                    from v in Split(_images.Height, PuzzleSize)
                    let rect = new Rect(h.start, v.start, h.size, v.size)
                    select new Cell(h.index, v.index, _images, rect);
                Parallel.ForEach(bots, bot => _bots.Add(bot));

                var groupActivities = new GroupActivities(_queue) {
                    Activities = _bots.Select(cell => new LoaderActivity(cell, _bots, _broadcast))
                };
                _queue.AddSubtask(this, groupActivities);
            }
            finally {
                _broadcast.End(this);
            }
        });

        private static IEnumerable<(int index, int start, int size)> Split(int totalSize, double maxSize) {
            var index = 0;
            var span = 0;
            var count = (int)Math.Ceiling(totalSize / maxSize);

            while (count > 0) {
                var size = (totalSize - span) / count;

                yield return (index, span, size);

                span += size;
                count--;
                index++;
            }
        }
    }
}
