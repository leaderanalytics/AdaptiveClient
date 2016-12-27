using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Autofac;
using Autofac.Builder;

namespace LeaderAnalytics.AdaptiveClient
{
    public class AutofacRegistrationHelper : IRegistrationHelper
    {
        private ContainerBuilder builder;
        private Dictionary<string, IPerimeter> EndPointDict;

        public AutofacRegistrationHelper(ContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            this.builder = builder;
            builder.RegisterModule(new AutofacModule());
            EndPointDict = new Dictionary<string, IPerimeter>();
        }

        public void RegisterEndPoints(IEnumerable<IEndPointConfiguration> endPoints)
        {
            if (endPoints == null)
                throw new ArgumentNullException("endPoints");
            
            // Do not register endpoints with the container.  A list of endpoints is available
            // when an EndPointCollection is resolved.
            
            foreach (var endPointCollection in endPoints.GroupBy(x => x.API_Name))
                EndPointDict.Add(endPointCollection.Key, new Perimeter(endPointCollection.Key, endPointCollection.ToList()));
        }

        /// <summary>
        /// Registers a service.  Call RegisterEndPoints before calling this method.
        /// </summary>
        /// <typeparam name="TService">Type of service i.e. OrdersService</typeparam>
        /// <typeparam name="TInterface">Interface of service i.e. IOrdersService</typeparam>
        /// <param name="endPointType">Type of client that will access this service i.e. HTTP, InProcess, WCF</param>
        /// <param name="endPointCollectionName">Name of server collection that exposes TInterface</param>
        public IRegistrationBuilder<TService, ConcreteReflectionActivatorData, SingleRegistrationStyle> Register<TService, TInterface>(EndPointType endPointType, string endPointCollectionName)
        {
            RegisterEndPointCollection(typeof(TInterface), endPointCollectionName);
            
            builder.Register<Func<EndPointType, TInterface>>(c => { IComponentContext cxt = c.Resolve<IComponentContext>(); return ept => cxt.ResolveKeyed<TInterface>(ept); });

            if (endPointType == EndPointType.WCF)
            {
                builder.Register<Func<string, ChannelFactory<TInterface>>>(c =>
                {
                    IComponentContext cxt = c.Resolve<IComponentContext>();
                    Func<IEndPointConfiguration> epfactory = cxt.Resolve<Func<IEndPointConfiguration>>();
                    IEndPointConfiguration ep = epfactory();

                    // Todo: need to resolve ChannelFactory

                    return s => 
                    new ChannelFactory<TInterface>(
                        new BasicHttpBinding(),
                        new EndpointAddress(ep.ConnectionString + s)
                        );
                });
            }
            return builder.RegisterType<TService>().Keyed<TInterface>(endPointType);
        }

        /// <summary>
        /// Register EndPointCollection by name using service interface as key. 
        /// </summary>
        /// <param name="serviceInterface">Type of interface i.e. IUsersService</param>
        /// <param name="endPointCollectionName">Name of EndPoint Collection that implements passed service interface</param>
        private void RegisterEndPointCollection(Type serviceInterface, string endPointCollectionName)
        {
            IPerimeter epc;
            EndPointDict.TryGetValue(endPointCollectionName, out epc);

            if (epc == null)
                throw new Exception($"An EndPointCollection named {endPointCollectionName} was not found.  Call the RegisterEndPoints method before calling RegisterService with an EndPointCollection name. Also check your spelling Sam.");

            builder.RegisterInstance<IPerimeter>(epc).Keyed<IPerimeter>(serviceInterface);
        }
    }
}
