using System.Threading.Tasks;

namespace Mosaic.Queue
{
    internal interface IActivity
    {
        Task Run();
    }
}