using System.Threading.Tasks;

namespace Mosaic.Jobs {
    internal interface ICreator {
        Task Set(ILayerResult input);
    }
}