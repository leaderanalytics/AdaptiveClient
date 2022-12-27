namespace LeaderAnalytics.AdaptiveClient.Tests;

public class MySQL_EndPointValidator : IEndPointValidator
{
    public virtual bool IsInterfaceAlive(IEndPointConfiguration endPoint)
    {
        bool result = true;

        using (MySqlConnection con = new MySqlConnection(endPoint.ConnectionString))
        {
            try
            {
                con.Open(); // must specify an existing database name in connection string or it throws.
            }
            catch (Exception)
            {
                result = false;
            }
        }
        return result;
    }

    public virtual async Task<bool> IsInterfaceAliveAsync(IEndPointConfiguration endPoint)
    {
        bool result = true;

        using (MySqlConnection con = new MySqlConnection(endPoint.ConnectionString))
        {
            try
            {
                await con.OpenAsync(); // must specify an existing database name in connection string or it throws.
            }
            catch (Exception)
            {
                result = false;
            }
        }
        return result;
    }
}
