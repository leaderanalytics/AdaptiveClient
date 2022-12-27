namespace LeaderAnalytics.AdaptiveClient;

public interface IEndPointConfiguration
{
    string Name { get; set; }
    string API_Name { get; set; }
    int Preference { get; set; }
    String EndPointType { get; set; }
    string ConnectionString { get; set; }
    string ProviderName { get; set; }
    Dictionary<string, string> Parameters { get; set; }
    bool IsActive { get; set; }
}
