namespace LeaderAnalytics.AdaptiveClient.Tests;

public class Http_EndPointValidator : IEndPointValidator
{
    public virtual bool IsInterfaceAlive(IEndPointConfiguration endPoint) => IsInterfaceAlive(endPoint.ConnectionString);

    public virtual bool IsInterfaceAlive(string url)
    {
        bool success = false;
        HttpClient httpClient = new HttpClient();
        HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
        HttpResponseMessage response = httpClient.SendAsync(msg).Result;
        success = response.StatusCode == HttpStatusCode.OK;
        return success;
    }

    public virtual async Task<bool> IsInterfaceAliveAsync(IEndPointConfiguration endPoint) => await IsInterfaceAliveAsync(endPoint.ConnectionString);

    public virtual async Task<bool> IsInterfaceAliveAsync(string url)
    {
        bool success = false;
        HttpClient httpClient = new HttpClient();
        HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
        HttpResponseMessage response = await httpClient.SendAsync(msg);
        success = response.StatusCode == HttpStatusCode.OK;
        return success;
    }
}
