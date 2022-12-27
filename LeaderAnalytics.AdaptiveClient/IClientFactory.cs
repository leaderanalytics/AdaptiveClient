namespace LeaderAnalytics.AdaptiveClient;

public interface IClientFactory<T> 
{
    IEndPointConfiguration CachedEndPoint { get; }
    T Create(params string[] overrideNames);
}