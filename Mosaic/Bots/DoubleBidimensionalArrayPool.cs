using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mosaic.Bots {
    internal sealed class DoubleBidimensionalArrayPool {
        private readonly ConcurrentBag<Item> _items = new ConcurrentBag<Item>();

        public DoubleBidimensionalArrayPool(int externalCount) {
            ExternalCount = externalCount;
        }

        public int ExternalCount { get; }

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
            private readonly double[][] _array;

            internal Item(DoubleBidimensionalArrayPool owner) {
                _rented = true;

                _array = new double[owner.ExternalCount][];
                for (var i = 0; i < owner.ExternalCount; i++) {
                    _array[i] = new double[3];
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

            public Item Fill<T>(IEnumerable<T> items, Action<double[], T> action) {
                var tuples = items.Select((item, index) => (row: _array[index], item));

                Parallel.ForEach(tuples, tuple => {
                    action(tuple.row, tuple.item);
                });

                return this;
            }

            public void Return() {
                _rented = false;
            }

            public static implicit operator double[][] (Item self) => self?._array;
        }
    }
}