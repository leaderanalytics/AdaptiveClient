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
            Func<EndPointType, T> serviceFactory,
            Func<EndPointType, IEndPointValidator> validatorFactory,
            EndPointCache endPointCache,
            EndPointContext endPointContext) : base(epcFactory, serviceFactory, validatorFactory, endPointCache, endPointContext)
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

            if (CurrentEndPoint != null)
            {
                endPointContext.CurrentEndPoint = CurrentEndPoint;
                return serviceFactory(CurrentEndPoint.EndPointType);
            }

            foreach (T client in ClientEnumerator())
            {
                IEndPointValidator validator = validatorFactory(CurrentEndPoint.EndPointType);

                if (!validator.IsInterfaceAlive(CurrentEndPoint))
                    continue;

                endPointContext.CurrentEndPoint = CurrentEndPoint;
                return client;
            }

            Hurl($"A functional EndPointConfiguration could not be resolved for client of type {typeof(T).Name}.", null);
            return default(T);
        }
    }
}
