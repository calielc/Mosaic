using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace Mosaic.Pools {
    [DebuggerDisplay("Count: {_items.Count}")]
    internal sealed class ArrayOfArrayPool<T> {
        private readonly ConcurrentBag<Item> _items = new ConcurrentBag<Item>();

        public Item Rent(int externalCount, int internalCount) {
            Item result;
            lock (_items) {
                result = _items.FirstOrDefault(item => item.ExternalCount == externalCount && item.InternalCount == internalCount && item.TryToRent());
                if (result != null) {
                    return result;
                }
            }

            result = new Item(externalCount, internalCount);
            _items.Add(result);
            return result;
        }

        [DebuggerDisplay("External: {ExternalCount}, Internal: {InternalCount}, Rented: {_rented}")]
        public sealed class Item {
            private bool _rented;
            private readonly T[][] _array;

            internal Item(int externalCount, int internalCount) {
                ExternalCount = externalCount;
                InternalCount = internalCount;
                _rented = true;

                _array = new T[externalCount][];
                for (var i = 0; i < externalCount; i++) {
                    _array[i] = new T[internalCount];
                }
            }

            public int ExternalCount { get; }
            public int InternalCount { get; }

            public T[][] Array => _array;

            internal bool TryToRent() {
                if (_rented) {
                    return false;
                }

                _rented = true;
                return true;
            }

            public void Return() {
                _rented = false;
            }

            public static implicit operator T[][] (Item self) => self._array;

            public T[] this[int index] => _array[index];
        }
    }
}