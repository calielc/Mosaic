using System.Threading.Tasks;

namespace Mosaic.Creators {
    internal interface ICreator {
        Task Set(ILayerResult input);
        Task Flush(string filename);
    }
}