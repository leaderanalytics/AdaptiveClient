using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient
{
    public class EndPointConfiguration : IEndPointConfiguration
    {
        public string Name { get; set; }
        public string Collection_Name { get; set; }
        public int Preference { get; set; }
        public EndPointType EndPointType { get; set; }
        public string ConnectionString { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public bool IsActive { get; set; }

        public EndPointConfiguration()
        {
            //Parameters = new Dictionary<string, string>();
        }
    }

    //public class InProcessEndPoint : EndPointConfiguration { }
    //public class RESTEndPoint : EndPointConfiguration { }
    //public class WCFEndPoint : EndPointConfiguration { }
}
