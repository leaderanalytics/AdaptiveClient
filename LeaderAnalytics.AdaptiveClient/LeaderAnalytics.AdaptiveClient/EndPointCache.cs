using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LeaderAnalytics.AdaptiveClient
{

    /// <summary>
    /// This class is resolved once per application lifetime so it is basically a singleton.
    /// Contrast to EndPointContext which is resolved every LifetimeScope.
    /// </summary>
    public class EndPointCache
    {
        private Dictionary<string, IEndPointConfiguration> EndPoints;

        public EndPointCache()
        {
            EndPoints = new Dictionary<string, IEndPointConfiguration>();
        }

        public IEndPointConfiguration GetEndPoint(string apiName)
        {
            IEndPointConfiguration result = null;
            EndPoints.TryGetValue(apiName, out result);
            return result;
        }

        public void SetEndPoint(string apiName, IEndPointConfiguration endPoint)
        {
            if (string.IsNullOrEmpty(apiName))
                throw new ArgumentNullException(apiName);

            if(endPoint == null)
                EndPoints.Remove(apiName);
            else
                EndPoints[apiName] = endPoint;
        }
    }
}
