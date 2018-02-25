using System.Threading.Tasks;
using ActivityQueue;

namespace Mosaic.Savers {
    internal interface ISaver : IActivity {
        Task Set(ILayerResult input);
    }
}