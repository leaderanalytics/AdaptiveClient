using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient
{
    public class EndPointConfiguration : IEndPointConfiguration
    {
        public string Name { get; set; }
        public string API_Name { get; set; }
        public int Preference { get; set; }
        public String EndPointType { get; set; }
        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public bool IsActive { get; set; }
    }
}
