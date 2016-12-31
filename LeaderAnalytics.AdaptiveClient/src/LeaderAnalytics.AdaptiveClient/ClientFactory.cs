using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LeaderAnalytics.AdaptiveClient
{
    public class ClientFactory<T> : IClientFactory<T>
    {
        private Func<Type, IPerimeter> epcFactory;
        private Func<EndPointType, T> serviceFactory;
        private Func<EndPointType, IEndPointValidator> validatorFactory;
        private EndPointContext endPointContext;
        public IEndPointConfiguration CurrentEndPoint { get; private set; }

        public ClientFactory(
            Func<Type, IPerimeter> epcFactory,
            Func<EndPointType, T> serviceFactory,
            Func<EndPointType, IEndPointValidator> validatorFactory,
            EndPointContext endPointContext
            )
        {
            this.epcFactory = epcFactory;
            this.serviceFactory = serviceFactory;
            this.validatorFactory = validatorFactory;
            this.endPointContext = endPointContext;
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
            endPointContext.CurrentEndPoint = CurrentEndPoint = null;
            T client = default(T);
            IPerimeter perimeter = epcFactory(typeof(T));
            IEnumerable<IEndPointConfiguration> endPoints;

            if (perimeter == null)
                return client;

            if (overrideNames != null && overrideNames.Any())
                endPoints = perimeter.EndPoints.Where(x => overrideNames.Any(o => x.Name == o));
            else
                endPoints = perimeter.EndPoints;

            foreach (IEndPointConfiguration endPoint in endPoints)
            {
                IEndPointValidator validator = validatorFactory(endPoint.EndPointType);

                if (!validator.IsInterfaceAlive(endPoint))
                    continue;

                // must set CurrentEndPoint before calling serviceFactory
                endPointContext.CurrentEndPoint = CurrentEndPoint = endPoint;
                client = serviceFactory(endPoint.EndPointType);

                if (client == null)
                    continue;
                
                break;
            }
            return client;
        }
    }
}
