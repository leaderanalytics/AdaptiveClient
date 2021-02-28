using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace LeaderAnalytics.AdaptiveClient
{
    public static class EndPointUtilities
    {
        public static IEnumerable<IEndPointConfiguration> LoadEndPoints(string fileName, bool activeOnly = true, string sectionName = "EndPoints")
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (!File.Exists(fileName))
                throw new Exception($"File not found: {fileName}");

            var endpoints = new ConfigurationBuilder().AddJsonFile(fileName).Build().GetSection(sectionName).Get<IEnumerable<EndPointConfiguration>>()?.Where(x => (!activeOnly) || x.IsActive).ToList();
            return endpoints;
        }


        public static void ValidateEndPoints(IEnumerable<IEndPointConfiguration> endPoints)
        {
            if (endPoints.Any(x => string.IsNullOrEmpty(x.Name)))
                throw new Exception("One or more EndPointConfigurations has a blank name.  Name is required for all EndPointConfigurations");

            if (endPoints.Any(x => string.IsNullOrEmpty(x.API_Name)))
                throw new Exception("One or more EndPointConfigurations has a blank API_Name.  API_Name is required for all EndPointConfigurations");

            if (endPoints.Any(x => string.IsNullOrEmpty(x.EndPointType)))
                throw new Exception("One or more EndPointConfigurations has a blank EndPointType.  EndPointType is required for all EndPointConfigurations");

            if (endPoints.Any(x => string.IsNullOrEmpty(x.ProviderName)))
                throw new Exception("One or more EndPointConfigurations has a blank ProviderName.  ProviderName is required for all EndPointConfigurations");

            var dupes = endPoints.GroupBy(x => x.Name).Where(x => x.Count() > 1);  // Must be unique across all api_names

            if (dupes.Any())
                throw new Exception($"Duplicate EndPointConfiguration found. EndPoint Name: {dupes.First().Key}." + Environment.NewLine + "Each EndPointConfiguration must have a unique name.  Set the IsActive flag to false to bypass an EndPointConfiguration.");
        }
    }
}
