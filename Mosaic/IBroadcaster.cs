using System;

namespace Mosaic {
    public interface IBroadcaster {
        void Start(object sender, string text);

        void Step(object sender);
        void Progress(object sender, double perc);

        void End(object sender, TimeSpan elapsed);
    }
}