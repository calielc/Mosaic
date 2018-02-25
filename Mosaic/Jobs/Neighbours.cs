using System.Collections.Generic;
using System.Linq;

namespace Mosaic.Jobs {
    internal readonly struct Neighbours {
        public Neighbours(Cell cell, IReadOnlyCollection<Cell> allCells) {
            Left = allCells.SingleOrDefault(bot => bot.X == cell.X - 1 && bot.Y == cell.Y);
            Right = allCells.SingleOrDefault(bot => bot.X == cell.X + 1 && bot.Y == cell.Y);
            Top = allCells.SingleOrDefault(bot => bot.X == cell.X && bot.Y == cell.Y - 1);
            Bottom = allCells.SingleOrDefault(bot => bot.X == cell.X && bot.Y == cell.Y + 1);
        }

        public Cell Left { get; }
        public Cell Right { get; }
        public Cell Bottom { get; }
        public Cell Top { get; }
    }
}