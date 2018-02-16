using System;
using System.Collections.Concurrent;

namespace Mosaic {
    internal sealed class Broadcast {
        private static readonly ConcurrentDictionary<object, DateTime> StartsBySender = new ConcurrentDictionary<object, DateTime>();

        public Broadcast(IBroadcaster broadcaster) {
            Broadcaster = broadcaster;
        }

        public IBroadcaster Broadcaster { get; }

        internal void Start(object sender, string text) {
            StartsBySender.TryAdd(sender, DateTime.Now);
            Broadcaster?.Start(sender, text);
        }

        internal void Step(object sender) => Broadcaster?.Step(sender);

        internal void Progress(object sender, double perc) => Broadcaster?.Progress(sender, perc);

        internal void End(object sender) {
            var startedAt = StartsBySender[sender];
            Broadcaster?.End(sender, DateTime.Now - startedAt);
        }
    }
}