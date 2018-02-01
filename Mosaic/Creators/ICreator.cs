using System.Threading.Tasks;
using Mosaic.Bots;

namespace Mosaic.Creators {
    internal interface ICreator {
        Task Set(BotResult botResult);
        Task Flush(string filename);
    }
}