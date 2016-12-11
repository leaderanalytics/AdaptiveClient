using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LeaderAnalytics.AdaptiveClient
{
    public class ClientResolver<T> : IClientResolver<T>
    {
        private Func<Type, IEndPointCollection> epcFactory;
        private Func<EndPointType, T> serviceFactory;
        private Func<EndPointType, IEndPointValidator> validatorFactory;
        private EndPointContext endPointInstance;

        public ClientResolver(
            Func<Type, IEndPointCollection> epcFactory,
            Func<EndPointType, T> serviceFactory,
            Func<EndPointType, IEndPointValidator> validatorFactory,
            EndPointContext endPointInstance
            )
        {
            this.epcFactory = epcFactory;
            this.serviceFactory = serviceFactory;
            this.validatorFactory = validatorFactory;
            this.endPointInstance = endPointInstance;
        }

        /// <summary>
        /// Given a service interface (IOrdersService), we find an EndPointCollection that implements the interface.
        /// Given the EndPointCollection, we find an EndPoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T ResolveClient(params string[] overrideNames)
        {
            T client = default(T);
            IEndPointCollection epc = epcFactory(typeof(T));
            IEnumerable<IEndPointConfiguration> endPoints;

            if (epc == null)
                return client;

            if (overrideNames != null && overrideNames.Any())
                endPoints = epc.EndPoints.Where(x => overrideNames.Any(o => x.Name == o));
            else
                endPoints = epc.EndPoints;

            foreach (IEndPointConfiguration endPoint in endPoints)
            {
                IEndPointValidator validator = validatorFactory(endPoint.EndPointType);

                if (!validator.IsInterfaceAlive(endPoint))
                    continue;

                // must set CurrentEndPoint before calling serviceFactory
                endPointInstance.CurrentEndPoint = endPoint;
                client = serviceFactory(endPoint.EndPointType);

                if (client == null)
                    continue;
                
                break;
            }
            return client;
        }
    }
}
