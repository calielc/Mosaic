using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivityQueue;
using Mosaic.Savers;

namespace Mosaic.Jobs {
    internal sealed class PickerBot : IActivity {
        private readonly IReadOnlyCollection<Cell> _cells;
        private readonly ISaver _saver;
        private readonly Broadcast _broadcast;

        public PickerBot(IReadOnlyCollection<Cell> cells, ISaver saver, Broadcast broadcast) {
            _cells = cells;
            _saver = saver;
            _broadcast = broadcast;
        }

        public async Task Run() => await Task.Run(() => {
            _broadcast.Start(this, $"Resolving {_cells.Count} cells");
            try {
                var unresolved = new LinkedList<Cell>(_cells);

                var count = 0;
                double divisor = unresolved.Count;

                Pick();

                while (unresolved.Count > 0) {
                    Parallel.ForEach(unresolved, cell => cell.UpdateNeighbourhood());
                    Pick();
                }

                void Pick() {
                    var bestChoice = unresolved.OrderByDescending(cell => cell.Score).First();
                    bestChoice.Pick(_saver, count);

                    unresolved.Remove(bestChoice);

                    _broadcast.Progress(this, ++count / divisor);

                }
            }
            finally {
                _broadcast.End(this);
            }

        });
    }
}