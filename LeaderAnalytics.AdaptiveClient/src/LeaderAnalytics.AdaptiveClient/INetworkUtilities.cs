namespace LeaderAnalytics.AdaptiveClient
{
    public interface INetworkUtilities
    {
        bool IsConnectionStringValid(string connectionString);
        bool IsNetworkAvailable();
        bool VerifyDBServerConnectivity(string connectionString);
        bool VerifyHttpServerAvailability(string url);
    }
}