using System.Threading.Tasks;

namespace ActivityQueue
{
    public interface IActivity
    {
        Task Run();
    }
}