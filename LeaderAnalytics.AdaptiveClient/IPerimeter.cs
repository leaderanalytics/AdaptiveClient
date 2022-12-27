namespace LeaderAnalytics.AdaptiveClient;

public interface IPerimeter
{
    string API_Name { get; }
    IList<IEndPointConfiguration> EndPoints { get; }
}
