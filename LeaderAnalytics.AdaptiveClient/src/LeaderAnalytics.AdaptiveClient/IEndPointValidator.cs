

namespace LeaderAnalytics.AdaptiveClient
{
    public interface IEndPointValidator
    {
        bool IsInterfaceAlive(IEndPointConfiguration endPoint);
    }
}