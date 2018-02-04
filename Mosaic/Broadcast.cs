using System;
using System.Collections.Concurrent;

namespace Mosaic {
    internal static class Broadcast {
        private static readonly ConcurrentDictionary<object, DateTime> StartsBySender = new ConcurrentDictionary<object, DateTime>();
        public static IBroadcaster Instance { get; set; }

        internal static void Start(object sender, string text) {
            StartsBySender.TryAdd(sender, DateTime.Now);
            Instance?.Start(sender, text);
        }

        internal static void Step(object sender) => Instance?.Step(sender);

        internal static void Progress(object sender, double perc) => Instance?.Progress(sender, perc);

        internal static void End(object sender) {
            var startedAt = StartsBySender[sender];
            Instance?.End(sender, DateTime.Now - startedAt);
        }
    }
}