using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LeaderAnalytics.AdaptiveClient
{
    public class EndPointContext
    {
        private Dictionary<string, IEndPointConfiguration> EndPoints;

        public EndPointContext()
        {
            EndPoints = new Dictionary<string, IEndPointConfiguration>();
        }

        public IEndPointConfiguration GetCurrentEndPoint(string apiName)
        {
            IEndPointConfiguration result = null;
            EndPoints.TryGetValue(apiName, out result);
            return result;
        }

        public void SetCurrentEndPoint(string apiName, IEndPointConfiguration endPoint)
        {
            if (string.IsNullOrEmpty(apiName))
                throw new ArgumentNullException(apiName);

            if (endPoint == null)
                throw new ArgumentNullException("endPoint");

            EndPoints[apiName] = endPoint;
        }
    }
}
