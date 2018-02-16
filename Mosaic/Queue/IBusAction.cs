using System.Threading.Tasks;

namespace Mosaic.Queue
{
    internal interface IBusAction
    {
        Task Run();
    }
}