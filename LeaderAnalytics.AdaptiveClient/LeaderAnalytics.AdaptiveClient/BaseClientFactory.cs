using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient
{
    public abstract class BaseClientFactory<T>
    {
        protected Func<Type, IPerimeter> epcFactory;
        protected Func<string, string, T> serviceFactory;
        protected Func<string, string, IEndPointValidator> validatorFactory;
        protected EndPointCache endPointCache;
        protected EndPointContext endPointContext;
        protected readonly IPerimeter perimeter;
        protected readonly Action<string> logger;

        public IEndPointConfiguration CachedEndPoint
        {
            // "Current" and "Cached" are not the same -  do not set EndPointContext.CurentEndPoint here.
            get { return endPointCache.GetEndPoint(perimeter.API_Name); }

            protected set { endPointCache.SetEndPoint(perimeter.API_Name, value); }
        }

        protected IEnumerable<IEndPointConfiguration> AvailableEndPoints { get; set; }

        public BaseClientFactory(
           Func<Type, IPerimeter> epcFactory,
           Func<string, string, T> serviceFactory,
           Func<string, string, IEndPointValidator> validatorFactory,
           EndPointCache endPointCache,
           EndPointContext endPointContext,
           Action<string> logger)
        {
            this.epcFactory = epcFactory;
            this.serviceFactory = serviceFactory;
            this.validatorFactory = validatorFactory;
            this.endPointCache = endPointCache;
            this.endPointContext = endPointContext;
            this.logger = logger;

            perimeter = epcFactory(typeof(T));

            if (perimeter == null)
                throw new Exception($"A Perimeter object could not be resolved for type {typeof(T).Name}.  A possible reason for this error is that type {typeof(T).Name} was not registered with a specific API_Name.", null);

            if(perimeter.EndPoints == null || ! perimeter.EndPoints.Any())
                throw new Exception($"A Perimeter object with an API_Name of {perimeter.API_Name} was resolved for type {typeof(T).Name},  however the EndPointConfigurationItems collection is null or contains no entries.", null);
        }

        protected void SetAvailableEndPoints(string[] overrideNames)
        {
            if (overrideNames != null && overrideNames.Any())
                AvailableEndPoints = perimeter.EndPoints.Where(x => overrideNames.Any(o => x.Name == o)).OrderBy(x => x.Preference);
            else
                AvailableEndPoints = perimeter.EndPoints.OrderBy(x => x.Preference);

            if (!AvailableEndPoints.Any())
                throw new Exception($"No EndPointConfigurationItems were available to use for {typeof(T).Name}.  A possible reason for this exception is that no EndPointConfigurations were found with names that match the list of override names.", null);

            // Make sure the current EndPoint, if any, exists in the registered EndPoint list after being filtered for overrides.
            if (CachedEndPoint != null && !AvailableEndPoints.Any(x => x.Name == CachedEndPoint.Name))
                CachedEndPoint = null;

            endPointContext.CurrentEndPoint = CachedEndPoint; 
        }

        protected IEnumerable<T> ClientEnumerator()
        {
            // if CurrentEndPoint is not null it means we have used it before.  Reuse the cached CurrentEndPoint on the first iteration

            if (CachedEndPoint == null)
                CachedEndPoint = AvailableEndPoints.First();

            while (CachedEndPoint != null)
            {
                // endPointContext.CurrentEndPoint must be set at this point because calling serviceFactory will attempt to new up a T.
                endPointContext.CurrentEndPoint = CachedEndPoint;
                T client = serviceFactory(CachedEndPoint.EndPointType, CachedEndPoint.ProviderName);

                if (client != null)
                    yield return client;

                CachedEndPoint = AvailableEndPoints.FirstOrDefault(x => x.Preference > CachedEndPoint.Preference);
            }
        }

        public virtual void InvalidateCachedEndPoint()
        {
            // Will cause a call to ClientEnumerator to start with the first EndPoint
            CachedEndPoint = null;
        }
    }
}
