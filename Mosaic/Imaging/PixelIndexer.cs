using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mosaic.Imaging {
    public sealed class PixelIndexer {
        private readonly RGBColor[,] _matrix;

        public PixelIndexer(RGBColor[,] matrix) {
            _matrix = matrix;
        }

        public RGBColor this[int x, int y] {
            get {
                Debug.Assert(x >= _matrix.GetLowerBound(0), $"x >= {_matrix.GetLowerBound(0)}");
                Debug.Assert(x <= _matrix.GetUpperBound(0), $"x <= {_matrix.GetUpperBound(0)}");
                Debug.Assert(y >= _matrix.GetLowerBound(1), $"y >= {_matrix.GetLowerBound(1)}");
                Debug.Assert(y <= _matrix.GetUpperBound(1), $"y <= {_matrix.GetUpperBound(1)}");

                return _matrix[x, y];
            }
        }

        public IEnumerable<RGBColor> this[IRectangle rect]
            => from x in Enumerable.Range(rect.Left, rect.Width)
               from y in Enumerable.Range(rect.Top, rect.Height)
               select _matrix[x, y];
    }
}