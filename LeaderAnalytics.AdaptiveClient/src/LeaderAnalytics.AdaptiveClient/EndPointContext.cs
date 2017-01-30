using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LeaderAnalytics.AdaptiveClient
{
    public class EndPointContext
    {
        private Dictionary<Type, IEndPointConfiguration> EndPoints;

        public EndPointContext()
        {
            EndPoints = new Dictionary<Type, IEndPointConfiguration>();
        }

        public IEndPointConfiguration GetCurrentEndPoint<T>()
        {
            IEndPointConfiguration result = null;
            EndPoints.TryGetValue(typeof(T), out result);
            return result;
        }

        public void SetCurrentEndPoint<T>(IEndPointConfiguration endPoint)
        {
            if (endPoint == null)
                throw new ArgumentNullException("endPoint");

            EndPoints[typeof(T)] = endPoint;
        }
    }
}
