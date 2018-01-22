using System;
using System.Threading.Tasks;

namespace Mosaic {
    public struct For {
        public static IForRange Between(int fromInclusive, int toExcluclusive) {
            return new ForRange(fromInclusive, toExcluclusive);
        }

        public interface IForRange {
            ForRange Do(Action<int> code);
        }

        public interface IFor {
            void Execute(bool parallel);
        }

        public struct ForRange : IForRange, IFor {
            private readonly int _fromInclusive;
            private readonly int _toExclusive;
            private Action<int> _code;

            public ForRange(int fromInclusive, int toExclusive) {
                _fromInclusive = fromInclusive;
                _toExclusive = toExclusive;
                _code = _ => new NotImplementedException();

            }

            ForRange IForRange.Do(Action<int> code) {
                _code = code;
                return this;
            }

            public void Execute(bool parallel) {
                if (parallel) {
                    Parallel.For(_fromInclusive, _toExclusive, _code);
                }
                else {
                    for (var idx = _fromInclusive; idx < _toExclusive; idx++) {
                        _code(idx);
                    }
                }
            }
        }
    }
}