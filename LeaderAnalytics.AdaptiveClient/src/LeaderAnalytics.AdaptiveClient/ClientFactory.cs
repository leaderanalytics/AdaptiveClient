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
        public IEndPointConfiguration CurrentEndPoint
        {
            get
            {
                return endPointContext.GetCurrentEndPoint<T>();
            }

            private set
            {
                endPointContext.SetCurrentEndPoint<T>(value);
            }
        }

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
            if (CurrentEndPoint != null)
            {
                if (overrideNames == null || overrideNames.Contains(CurrentEndPoint.Name))
                    return serviceFactory(CurrentEndPoint.EndPointType);
            }

            T client = default(T);
            IPerimeter perimeter = epcFactory(typeof(T));
            IEnumerable<IEndPointConfiguration> endPoints;

            if (perimeter == null)
                return NullClient();

            if (overrideNames != null && overrideNames.Any())
                endPoints = perimeter.EndPoints.Where(x => overrideNames.Any(o => x.Name == o));
            else
                endPoints = perimeter.EndPoints;

            if (!endPoints.Any())
                return NullClient();


            foreach (IEndPointConfiguration endPoint in endPoints)
            {
                IEndPointValidator validator = validatorFactory(endPoint.EndPointType);

                if (!validator.IsInterfaceAlive(endPoint))
                    continue;

                CurrentEndPoint = endPoint;
                endPointContext.SetCurrentEndPoint<T>(endPoint);
                client = serviceFactory(endPoint.EndPointType);

                if (client == null)
                    continue;
                
                break;
            }
            return client;
        }

        public void CallClient(Action evaluator, params string[] overrideNames)
        {
            T client = default(T);
            IPerimeter perimeter = epcFactory(typeof(T));
            IEnumerable<IEndPointConfiguration> endPoints;

            if (perimeter == null)
                Hurl($"No EndPointConfigurationItems were registered to type {typeof(T).Name}", null);

            if (overrideNames != null && overrideNames.Any())
                endPoints = perimeter.EndPoints.Where(x => overrideNames.Any(o => x.Name == o));
            else
                endPoints = perimeter.EndPoints;

            if (!endPoints.Any())
                Hurl($"No EndPointConfigurationItems were available to use for {typeof(T).Name}.  A possible reason for this exception is that no EndPointConfigurations were found with names that match the list of override names.", null);


            // Make sure the current EndPoint, if any, exists in the registered EndPoint list after being filtered for overrides.
            if (CurrentEndPoint != null && !endPoints.Any(x => x.Name == CurrentEndPoint.Name))
                CurrentEndPoint = null;

            bool isClientFound = false;

            while (! isClientFound)
            {
                if (CurrentEndPoint == null)
                    CurrentEndPoint = endPoints.First();
                else
                    CurrentEndPoint = endPoints.FirstOrDefault(x => x.Preference > CurrentEndPoint.Preference);

                if (CurrentEndPoint == null)
                    Hurl("", null);

                client = serviceFactory(CurrentEndPoint.EndPointType);

                if (client == null)
                    continue;

                try
                {
                    evaluator();
                    isClientFound = true;
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

        private T NullClient()
        {
            CurrentEndPoint = null;
            return default(T);
        }

        private void Hurl(string message, Exception innerEx)
        {

        }
    }
}
