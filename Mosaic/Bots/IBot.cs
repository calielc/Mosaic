using System.Threading.Tasks;
using Mosaic.Creators;

namespace Mosaic.Bots {
    internal interface IBot {
        Task Process(ICreator creator);
    }
}