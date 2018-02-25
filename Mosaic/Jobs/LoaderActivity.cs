using System.Collections.Generic;
using System.Threading.Tasks;
using ActivityQueue;

namespace Mosaic.Jobs {
    internal sealed class LoaderActivity : IActivity {
        private readonly Cell _cell;
        private readonly IReadOnlyCollection<Cell> _allCells;
        private readonly Broadcast _broadcast;

        public LoaderActivity(Cell cell, IReadOnlyCollection<Cell> allCells, Broadcast broadcast) {
            _cell = cell;
            _allCells = allCells;
            _broadcast = broadcast;
        }

        public async Task Run() => await Task.Run(() => {
            _broadcast.Start(this, $"Loading Cell[{_cell.X:00}, {_cell.Y:00}]");
            try {
                _cell.Prepare(_allCells, (count, max) => {
                    _broadcast.Progress(this, ++count / max);
                });
            }
            finally {
                _broadcast.End(this);
            }
        });
    }
}