using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mosaic.Bots {
    internal sealed class ArrayOfArrayPool<T> {
        private readonly ConcurrentBag<Item> _items = new ConcurrentBag<Item>();

        public ArrayOfArrayPool(int externalCount, int internalCount) {
            ExternalCount = externalCount;
            InternalCount = internalCount;
        }

        public int ExternalCount { get; }
        public int InternalCount { get; }

        public Item Rent() {
            lock (_items) {
                var result = _items.FirstOrDefault(item => item.TryToRent());
                if (result != null) {
                    return result;
                }
            }

            return new Item(this);
        }

        public sealed class Item {
            private bool _rented;
            private readonly T[][] _array;

            internal Item(ArrayOfArrayPool<T> owner) {
                _rented = true;

                _array = new T[owner.ExternalCount][];
                for (var i = 0; i < owner.ExternalCount; i++) {
                    _array[i] = new T[owner.InternalCount];
                }

                owner._items.Add(this);
            }

            internal bool TryToRent() {
                if (_rented) {
                    return false;
                }

                _rented = true;
                return true;
            }

            public Item Fill<T2>(IEnumerable<T2> items, Action<T[], T2> action) {
                var tuples = items.Select((item, index) => (row: _array[index], item));

                Parallel.ForEach(tuples, tuple => {
                    action(tuple.row, tuple.item);
                });

                return this;
            }

            public void Return() {
                _rented = false;
            }

            public static implicit operator T[][] (Item self) => self?._array;
        }
    }
}