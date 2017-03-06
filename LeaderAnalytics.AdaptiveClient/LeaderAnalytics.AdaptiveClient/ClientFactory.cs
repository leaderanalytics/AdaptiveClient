using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LeaderAnalytics.AdaptiveClient
{
    public class ClientFactory<T> :BaseClientFactory<T>, IClientFactory<T>
    {
        public ClientFactory(
            Func<Type, IPerimeter> epcFactory,
            Func<string, T> serviceFactory,
            Func<string, IEndPointValidator> validatorFactory,
            EndPointCache endPointCache,
            EndPointContext endPointContext,
            Action<string> logger) : base(epcFactory, serviceFactory, validatorFactory, endPointCache, endPointContext, logger)
        {
        
        }

        /// <summary>
        /// Given an interface, we find the Perimeter with whom the interface is registered.
        /// Given the Perimeter, we find an EndPointConfiguration that is alive.
        /// Given the EndPointType of
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Create(params string[] overrideNames)
        {
            SetAvailableEndPoints(overrideNames);
            IEndPointConfiguration cachedEndPoint = CachedEndPoint;

            if (cachedEndPoint != null)
            {
                return serviceFactory(cachedEndPoint.EndPointType);
            }

            foreach (T client in ClientEnumerator())
            {
                IEndPointValidator validator = validatorFactory(CachedEndPoint.EndPointType);

                if (!validator.IsInterfaceAlive(CachedEndPoint))
                {
                    if (logger != null)
                        logger($"Failed to connect to EndPoint named {CachedEndPoint.Name} when resolving a client of type {typeof(T)}.");

                    continue;
                }
                return client;
            }

            Hurl($"A functional EndPointConfiguration could not be resolved for client of type {typeof(T).Name}.", null);
            return default(T);
        }
    }
}
