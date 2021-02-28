using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<string, IEndPointConfiguration> EndPoints;
        private ConcurrentDictionary<string, bool> ValidatedEndPoints;

        public EndPointCache()
        {
            EndPoints = new ConcurrentDictionary<string, IEndPointConfiguration>();
            ValidatedEndPoints = new ConcurrentDictionary<string, bool>();
        }

        public IEndPointConfiguration GetEndPoint(string apiName)
        {
            EndPoints.TryGetValue(apiName, out IEndPointConfiguration result);
            return result;
        }

        public void SetEndPoint(string apiName, IEndPointConfiguration endPoint)
        {
            if (string.IsNullOrEmpty(apiName))
                throw new ArgumentNullException(apiName);
            
            if(endPoint == null)
                EndPoints.TryRemove(apiName, out IEndPointConfiguration dummy);
            else
                EndPoints[apiName] = endPoint;
        }

        public void SetValidationResult(string endPointName, bool result)
        {
            ValidatedEndPoints[endPointName] = result;
        }

        public bool? GetValidationResult(string endPointName)
        {
            bool isValidated = ValidatedEndPoints.TryGetValue(endPointName, out bool result);

            if (!isValidated)
                return null;

            return new Nullable<bool>(result);
        }

    }
}
