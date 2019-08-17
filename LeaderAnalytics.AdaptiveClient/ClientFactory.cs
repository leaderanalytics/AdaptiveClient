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
            Func<string, string, T> serviceFactory,
            Func<string, string, IEndPointValidator> validatorFactory,
            EndPointCache endPointCache,
            EndPointContext endPointContext,
            Action<string> logger) : base(epcFactory, serviceFactory, validatorFactory, endPointCache, endPointContext, logger)
        {
        
        }

        /// <summary>
        /// Given an interface, we find the Perimeter with whom the interface is registered.
        /// Using the Perimeter, we find an EndPointConfiguration that is alive.
        /// Using the EndPointType and ProviderName of the resolved EndPointConfiguration, we find an implementation of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Create(params string[] overrideNames)
        {
            SetAvailableEndPoints(overrideNames);
            IEndPointConfiguration cachedEndPoint = CachedEndPoint;

            if (cachedEndPoint != null)
                return serviceFactory(cachedEndPoint.EndPointType, cachedEndPoint.ProviderName);

            foreach (T client in ClientEnumerator())
            {
                bool? validationResult = endPointCache.GetValidationResult(CachedEndPoint.Name);

                if (validationResult.HasValue)  // .HasValue when validated previously
                {
                    if (validationResult.Value)
                        return client;   
                    else
                        continue;       
                } 

                IEndPointValidator validator = validatorFactory(CachedEndPoint.EndPointType, CachedEndPoint.ProviderName);
                bool isAlive = validator.IsInterfaceAlive(CachedEndPoint);
                endPointCache.SetValidationResult(CachedEndPoint.Name, isAlive);

                if (! isAlive)
                {
                    logger?.Invoke($"Failed to connect to EndPoint named {CachedEndPoint.Name} when resolving a client of type {typeof(T)}.");
                    continue;
                }
                return client;
            }

            throw new Exception($"A functional EndPointConfiguration could not be resolved for client of type {typeof(T).Name}.", null);
        }
    }
}
