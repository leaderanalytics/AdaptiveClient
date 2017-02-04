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
            EndPointContext endPointContext) : base(epcFactory, serviceFactory, validatorFactory, endPointContext)
        {
        
        }

        /// <summary>
        /// Given a service interface (IOrdersService), we find a Perimeter that implements the interface.
        /// Given the Perimeter, we find an EndPoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Create(params string[] overrideNames)
        {
            SetAvailableEndPoints(overrideNames);

            if(CurrentEndPoint != null)
                return serviceFactory(CurrentEndPoint.EndPointType);

            foreach (T client in ClientEnumerator())
            {
                IEndPointValidator validator = validatorFactory(CurrentEndPoint.EndPointType);

                if (!validator.IsInterfaceAlive(CurrentEndPoint))
                    continue;

                return client;
            }

            Hurl($"A functional EndPointConfiguration could not be resolved for client of type {typeof(T).Name}.", null);
            return default(T);
        }
    }
}
