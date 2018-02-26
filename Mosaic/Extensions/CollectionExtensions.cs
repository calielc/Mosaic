using System.Collections.Generic;

namespace Mosaic.Extensions {
    internal static class CollectionExtensions {
        public static TCollection AddIf<TCollection, TItem>(this TCollection self, bool conditional, TItem value) where TCollection : ICollection<TItem> {
            if (conditional) {
                self.Add(value);
            }

            return self;
        }
    }
}