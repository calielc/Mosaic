using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mosaic.Imaging;
using Mosaic.Savers;

namespace Mosaic.Jobs {
    [DebuggerDisplay("Pos: ({X}, {Y}) - Rect: ({_rect.Left}, {_rect.Top}, {_rect.Width}, {_rect.Height}) - IsResolved: {IsResolved} - Score: {Score}")]
    internal class Cell {
        private readonly ImageCollection _images;
        private readonly IRectangle _rect;
        private Neighbours _neighbours;
        private Frame[] _frames;
        private Frame _bestChoice;

        public Cell(int x, int y, ImageCollection images, IRectangle rect) {
            X = x;
            Y = y;
            _rect = rect;
            _images = images;
        }

        public int X { get; }
        public int Y { get; }

        public bool IsResolved { get; private set; }

        public double Score => _bestChoice?.Score ?? -1d;

        public void Prepare(IReadOnlyCollection<Cell> cells, Action<int, double> broadcast) {
            _neighbours = new Neighbours(this, cells);

            _frames = _images
                .Select(image => new Frame(image, _rect))
                .ToArray();

            var frameCount = 0;
            double frameDivisor = _frames.Length;

            foreach (var frame in _frames) {
                frame.UpdateConstancy(_frames);

                broadcast(++frameCount, frameDivisor);
            }

            _bestChoice = _frames.OrderByDescending(frame => frame.Score).FirstOrDefault();
        }

        public void UpdateBestChoice() {
            if (IsResolved) {
                return;
            }

            IEnumerable<Frame> frames = _frames;

            CompareNeighbour(_neighbours.Left, neighbour => neighbour.Right, frame => frame.Left);
            CompareNeighbour(_neighbours.Right, neighbour => neighbour.Left, frame => frame.Right);

            CompareNeighbour(_neighbours.Top, neighbour => neighbour.Bottom, frame => frame.Top);
            CompareNeighbour(_neighbours.Bottom, neighbour => neighbour.Top, frame => frame.Bottom);

            _bestChoice = frames.OrderByDescending(frame => frame.Score).FirstOrDefault();

            void CompareNeighbour(Cell neighbour, Func<FrameBorders, FrameBorder> getNeighbourBorder, Func<FrameBorders, FrameBorder> getFrameBorder) {
                if (neighbour == null || neighbour.IsResolved == false) {
                    return;
                }

                var neighbourBorder = getNeighbourBorder(neighbour._bestChoice.Borders);

                frames = frames.Where(frame => getFrameBorder(frame.Borders) % neighbourBorder > 90).ToArray();
            }
        }

        public void Resolve(ISaver saver, int order) {
            IsResolved = true;
            var bestChoice = _bestChoice;

            double score;
            if (bestChoice == null) {
                bestChoice = _frames.OrderByDescending(frame => frame.Score).First();
                score = bestChoice.Score / 100d;
            }
            else {
                score = bestChoice.Score;
            }

            using (var result = new LayerResult(_rect)) {
                result.Order = order;
                result.Name = bestChoice.Name;
                result.Score = score;
                bestChoice.CopyTo(result);

                saver.Set(result);
            }
        }
    }
}