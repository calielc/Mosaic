using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mosaic.Imaging;

namespace Mosaic.Jobs {
    [DebuggerDisplay("Name: {Name}, Constancy: {Constancy}, Score: {Score}")]
    internal sealed class Frame {
        private readonly Image _image;
        private readonly IRectangle _rect;
        private readonly ColorHistogram _histogram;

        public Frame(Image image, IRectangle rect) {
            _image = image;
            _rect = rect;

            _histogram = new ColorHistogram(image.Reduced[rect]);

            Borders = new FrameBorders(image, rect);
        }

        public string Name => _image.Name;

        public FrameBorders Borders { get; }

        public double Constancy { get; private set; }

        public double Score => Constancy;

        public void UpdateConstancy(IEnumerable<Frame> frames) {
            Constancy = frames.Average(frame => this % frame);
        }

        public static double operator %(Frame left, Frame right) {
            if (ReferenceEquals(left._image, right._image)) {
                return 100d;
            }

            const double half = 0.5d;
            return left._histogram % right._histogram * half +
                   right.Borders % right.Borders * half;
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