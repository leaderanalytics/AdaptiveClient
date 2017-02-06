using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient
{
    public abstract class BaseClientFactory<T>
    {
        protected Func<Type, IPerimeter> epcFactory;
        protected Func<EndPointType, T> serviceFactory;
        protected Func<EndPointType, IEndPointValidator> validatorFactory;
        protected EndPointCache endPointCache;
        protected EndPointContext endPointContext;
        protected readonly IPerimeter perimeter;

        public IEndPointConfiguration CurrentEndPoint
        {
            get
            {
                return endPointCache.GetCurrentEndPoint(perimeter.API_Name);
            }

            protected set
            {
                endPointCache.SetCurrentEndPoint(perimeter.API_Name, value);
            }
        }
        protected IEnumerable<IEndPointConfiguration> AvailableEndPoints { get; set; }

        public BaseClientFactory(
           Func<Type, IPerimeter> epcFactory,
           Func<EndPointType, T> serviceFactory,
           Func<EndPointType, IEndPointValidator> validatorFactory,
           EndPointCache endPointCache,
           EndPointContext endPointContext)
        {
            this.epcFactory = epcFactory;
            this.serviceFactory = serviceFactory;
            this.validatorFactory = validatorFactory;
            this.endPointCache = endPointCache;
            this.endPointContext = endPointContext;

            perimeter = epcFactory(typeof(T));

            if (perimeter == null)
                Hurl($"A Perimeter object could not be resolved for type {typeof(T).Name}.  A possible reason for this error is that type {typeof(T).Name} was not registered with a specific API_Name.", null);

            if(perimeter.EndPoints == null || ! perimeter.EndPoints.Any())
                Hurl($"A Perimeter object with an API_Name of {perimeter.API_Name} was resolved for type {typeof(T).Name},  however the EndPointConfigurationItems collection is null or contains no entries.", null);
        }

        protected void Hurl(string message, Exception innerEx)
        {
            Exception ex = new Exception(message, innerEx);
            throw ex;
        }

        protected void SetAvailableEndPoints(string[] overrideNames)
        {
            if (overrideNames != null && overrideNames.Any())
                AvailableEndPoints = perimeter.EndPoints.Where(x => overrideNames.Any(o => x.Name == o)).OrderBy(x => x.Preference);
            else
                AvailableEndPoints = perimeter.EndPoints.OrderBy(x => x.Preference);

            if (!AvailableEndPoints.Any())
                Hurl($"No EndPointConfigurationItems were available to use for {typeof(T).Name}.  A possible reason for this exception is that no EndPointConfigurations were found with names that match the list of override names.", null);

            // Make sure the current EndPoint, if any, exists in the registered EndPoint list after being filtered for overrides.
            if (CurrentEndPoint != null && !AvailableEndPoints.Any(x => x.Name == CurrentEndPoint.Name))
                CurrentEndPoint = null;
        }

        protected IEnumerable<T> ClientEnumerator()
        {
            // if CurrentEndPoint is not null it means we have used it before.  Reuse the cached CurrentEndPoint on the first iteration
            bool useCachedEndPoint = CurrentEndPoint != null;
            int tryCount = useCachedEndPoint ? AvailableEndPoints.Count(x => x.Preference >= CurrentEndPoint.Preference) : AvailableEndPoints.Count();

            for(int i=0;i<tryCount;i++)
            {
                T client = default(T); // remove
                 
                if (!useCachedEndPoint)
                    CurrentEndPoint = AvailableEndPoints.ElementAt(i);
                try
                {
                    useCachedEndPoint = false;
                     client = serviceFactory(CurrentEndPoint.EndPointType);

                    if (client == null)
                        continue; // not an error - just means no client is registered for this endpoint.
                }
                catch (Exception ex)
                {
                    string w = ex.Message;
                }
                yield return client;
            }
        }

        public virtual void InvalidateCurrentEndPoint()
        {
            // Will cause a call to ClientEnumerator to start with the first EndPoint
            CurrentEndPoint = null;
        }
    }
}
