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
        /// Given a service interface (IOrdersService), we find a Perimeter that implements the interface.
        /// Given the Perimeter, we find an EndPoint.
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


        private T CreateClient(Func<T> evaluator, params string[] overrideNames)
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
                // must set CurrentEndPoint before calling serviceFactory
                endPointContext.CurrentEndPoint = CurrentEndPoint = endPoint;
                client = serviceFactory(endPoint.EndPointType);

                if (client == null)
                    continue;

                bool isClientFound = true;

                try
                {
                    evaluator();
                }
                catch (Exception ex)
                {
                    isClientFound = false;
                }
                

                if (!isClientFound)
                    continue;
                
                break;
            }
            return client;
        }

        private bool CallClient<TResult>(Func<T, TResult> client, IEndPointConfiguration ep)
        {
            bool result = true;

            try
            {
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result; 
        }

        private bool CallValidator(IEndPointValidator validator, IEndPointConfiguration ep)
        {
            return validator.IsInterfaceAlive(ep);
        }
    }
}
