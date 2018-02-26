using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mosaic.Extensions;

namespace Mosaic.Jobs {
    internal readonly struct Neighbours : IDirection<Cell>, IEnumerable<Cell> {
        private readonly IReadOnlyCollection<Cell> _all;

        public Neighbours(Cell cell, IReadOnlyCollection<Cell> allCells) {
            Left = allCells.SingleOrDefault(bot => bot.X == cell.X - 1 && bot.Y == cell.Y);
            Right = allCells.SingleOrDefault(bot => bot.X == cell.X + 1 && bot.Y == cell.Y);
            Top = allCells.SingleOrDefault(bot => bot.X == cell.X && bot.Y == cell.Y - 1);
            Bottom = allCells.SingleOrDefault(bot => bot.X == cell.X && bot.Y == cell.Y + 1);

            _all = new List<Cell>(4)
                .AddIf(Left != null, Left)
                .AddIf(Right != null, Right)
                .AddIf(Top != null, Top)
                .AddIf(Bottom != null, Bottom);
        }

        public Cell Left { get; }
        public Cell Right { get; }
        public Cell Bottom { get; }
        public Cell Top { get; }

        public IEnumerator<Cell> GetEnumerator() => _all.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_all).GetEnumerator();
    }
}