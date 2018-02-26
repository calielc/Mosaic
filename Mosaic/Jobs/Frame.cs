using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mosaic.Extensions;
using Mosaic.Imaging;

namespace Mosaic.Jobs {
    [DebuggerDisplay("Name: {Name}, Constancy: {_constancy}, Neighbourhood: {_neighbourhood}, Score: {Score}")]
    internal sealed class Frame {
        private readonly Image _image;
        private readonly IRectangle _rect;
        private readonly ColorHistogram _histogram;
        private readonly FrameBorders _borders;
        private double _rateContancy = 1d;
        private double _rateNeighbourhood;
        private double _constancy = 100d;
        private double _neighbourhood = 100d;

        public Frame(Image image, IRectangle rect) {
            _image = image;
            _rect = rect;

            _histogram = new ColorHistogram(image.Reduced[rect]);

            _borders = new FrameBorders(image, rect);
        }

        public string Name => _image.Name;

        public double Score => _constancy * _rateContancy + _neighbourhood * _rateNeighbourhood;

        public void UpdateConstancy(IEnumerable<Frame> frames) {
            _constancy = frames.Average(frame => this % frame);
            UpdateRating(1d);
        }

        public void UpdateNeighbourhood(Neighbours neighbours) {
            _neighbourhood = new[] {
                Calc(Direction.Left),
                Calc(Direction.Right),
                Calc(Direction.Top),
                Calc(Direction.Bottom)
            }.HarmonicAverage();

            UpdateRating(0.1d);

            double Calc(Direction direction) {
                var neighbour = neighbours.GetByDirection(direction);
                if (neighbour is null || neighbour.IsResolved == false) {
                    return 100d;
                }

                var neighbourBorder = neighbour.SelectFrame._borders.GetByOppositeDirection(direction);
                var frameBorder = _borders.GetByDirection(direction);

                var result = neighbourBorder % frameBorder;
                if (Name != neighbour.SelectFrame.Name) {
                    return result;
                }

                return result * 1.0005d;
            }
        }

        private void UpdateRating(double contancy) {
            _rateContancy = contancy;
            _rateNeighbourhood = 1d - _rateContancy;
        }


        public static double operator %(Frame left, Frame right) {
            if (ReferenceEquals(left._image, right._image)) {
                return 100d;
            }

            const double half = 0.5d;
            return left._histogram % right._histogram * half +
                   right._borders % right._borders * half;
        }

        public void CopyTo(LayerResult result) {
            int x, y;
            int posX, posY;
            for (x = 0, posX = _rect.Left; x < _rect.Width; x++, posX++) {
                for (y = 0, posY = _rect.Top; y < _rect.Height; y++, posY++) {
                    result.Set(x, y, _image.Raw[posX, posY]);
                }
            }
        }
    }
}