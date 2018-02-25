using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivityQueue;
using Mosaic.Savers;

namespace Mosaic.Jobs {
    internal sealed class PickerBot : IActivity {
        private readonly IReadOnlyCollection<Cell> _bots;
        private readonly ISaver _saver;
        private readonly Broadcast _broadcast;

        public PickerBot(IReadOnlyCollection<Cell> bots, ISaver saver, Broadcast broadcast) {
            _bots = bots;
            _saver = saver;
            _broadcast = broadcast;
        }

        public async Task Run() => await Task.Run(() => {
            _broadcast.Start(this, $"Resolving {_bots.Count} bots");
            try {
                var count = 0;
                double divisor = _bots.Count;

                var unresolved = _bots.Where(bot => bot.IsResolved == false).ToArray();
                while (unresolved.Any()) {
                    Parallel.ForEach(unresolved, cell => cell.UpdateBestChoice());

                    var bestChoice = unresolved.OrderByDescending(bot => bot.Score).First();
                    bestChoice.Resolve(_saver, count);

                    _broadcast.Progress(this, ++count / divisor);

                    unresolved = unresolved.Where(bot => bot.IsResolved == false).ToArray();
                }
            }
            finally {
                _broadcast.End(this);
            }
        });
    }
}