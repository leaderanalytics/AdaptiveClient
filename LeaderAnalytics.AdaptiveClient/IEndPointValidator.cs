using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient
{
    public interface IEndPointValidator
    {
        bool IsInterfaceAlive(IEndPointConfiguration endPoint);
        Task<bool> IsInterfaceAliveAsync(IEndPointConfiguration endPoint);
    }
}