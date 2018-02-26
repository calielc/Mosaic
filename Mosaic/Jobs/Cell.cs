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

        public Cell(int x, int y, ImageCollection images, IRectangle rect) {
            X = x;
            Y = y;
            _rect = rect;
            _images = images;
        }

        public int X { get; }
        public int Y { get; }

        public bool IsResolved { get; private set; }

        internal Frame SelectFrame { get; private set; }

        public double Score => SelectFrame?.Score ?? -1d;

        public void Prepare(IReadOnlyCollection<Cell> cells, Action<int, double> broadcast) {
            Debug.Assert(IsResolved == false, "IsResolved == false");

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

            SelectFrame = _frames.OrderByDescending(frame => frame.Score).First();
        }

        public void UpdateNeighbourhood() {
            Debug.Assert(IsResolved == false, "IsResolved == false");

            if (_neighbours.All(cell => cell.IsResolved == false)) {
                SelectFrame = null;
                return;
            }

            foreach (var frame in _frames) {
                frame.UpdateNeighbourhood(_neighbours);
            }

            SelectFrame = _frames.OrderByDescending(frame => frame.Score).First();
        }

        public void Pick(ISaver saver, int order) {
            IsResolved = true;
            var choice = SelectFrame;

            double score;
            if (choice == null) {
                choice = _frames.OrderByDescending(frame => frame.Score).First();
                score = choice.Score / 100d;
            }
            else {
                score = choice.Score;
            }

            using (var result = new LayerResult(_rect)) {
                result.Order = order;
                result.Name = choice.Name;
                result.Score = score;
                choice.CopyTo(result);

                saver.Set(result);
            }
        }
    }
}