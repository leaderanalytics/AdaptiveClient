using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient.Tests
{
    public interface IDummyAPI1: IDisposable
    {
        string GetString();  
    }

    public interface IDummyAPI2: IDisposable
    {
        int GetInt();
    }

    public class InProcessClient1 : IDummyAPI1
    {
        public string GetString()
        {
            return "InProcessClient1";
        }

        public void Dispose()
        {

        }
    }

    public class WebAPIClient1 : IDummyAPI1
    {
        public string GetString()
        {
            return "WebAPIClient1";
        }

        public void Dispose()
        {

        }
    }


    public class InProcessClient2 : IDummyAPI2
    {
        public int GetInt()
        {
            return 1;
        }

        public void Dispose()
        {

        }

    }

    public class WebAPIClient2 : IDummyAPI2
    {
        public int GetInt()
        {
            return 2;
        }

        public void Dispose()
        {

        }
    }
}
