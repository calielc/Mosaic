using System;
using System.Collections.Concurrent;

namespace Mosaic {
    internal sealed class Broadcast {
        private static readonly ConcurrentDictionary<object, DateTime> StartsBySender = new ConcurrentDictionary<object, DateTime>();
        private readonly IBroadcaster _broadcaster;

        public Broadcast(IBroadcaster broadcaster) {
            _broadcaster = broadcaster;
            _broadcaster = broadcaster;
        }

        internal void Start(object sender, string text) {
            StartsBySender.TryAdd(sender, DateTime.Now);
            _broadcaster?.Start(sender, text);
        }

        internal void Step(object sender) => _broadcaster?.Step(sender);

        internal void Progress(object sender, double perc) => _broadcaster?.Progress(sender, perc);

        internal void End(object sender) {
            var startedAt = StartsBySender[sender];
            _broadcaster?.End(sender, DateTime.Now - startedAt);
        }
    }
}