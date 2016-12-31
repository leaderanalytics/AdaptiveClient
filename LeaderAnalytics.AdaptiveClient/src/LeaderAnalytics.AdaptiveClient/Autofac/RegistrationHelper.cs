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

            // Do not register endpoints with the container.  A list of endpoints is available when an Perimeter is resolved.
            endPoints = endPoints.Where(x => x.IsActive);
            ValidateEndPoints(endPoints);

            foreach (var perimeter in endPoints.GroupBy(x => x.API_Name))
                EndPointDict.Add(perimeter.Key, new Perimeter(perimeter.Key, perimeter.ToList()));
        }

        /// <summary>
        /// Registers a client. Call RegisterEndPoints before calling this method.
        /// </summary>
        /// <typeparam name="TClient">Concrete class that implements TInterface i.e. OrdersClient</typeparam>
        /// <typeparam name="TInterface">Interface of service i.e. IOrdersService</typeparam>
        /// <param name="endPointType">Type of client that will access this service i.e. WebAPI, InProcess, WCF</param>
        /// <param name="api_name">API_Name of EndPointConfiguration objects  TInterface</param>
        public IRegistrationBuilder<TClient, ConcreteReflectionActivatorData, SingleRegistrationStyle> Register<TClient, TInterface>(EndPointType endPointType, string api_name)
        {
            RegisterPerimeter(typeof(TInterface), api_name);
            
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
            return builder.RegisterType<TClient>().Keyed<TInterface>(endPointType);
        }

        /// <summary>
        /// Register Perimeter by name using service interface as key. 
        /// </summary>
        /// <param name="serviceInterface">Type of interface i.e. IUsersService</param>
        /// <param name="api_name">Name of API that implements passed service interface</param>
        private void RegisterPerimeter(Type serviceInterface, string api_name)
        {
            IPerimeter perimeter;
            EndPointDict.TryGetValue(api_name, out perimeter);

            if (perimeter == null)
                throw new Exception($"A Perimeter named {api_name} was not found.  Call the RegisterEndPoints method before calling Register. Also check your spelling.");

            builder.RegisterInstance<IPerimeter>(perimeter).Keyed<IPerimeter>(serviceInterface);
        }

        private void ValidateEndPoints(IEnumerable<IEndPointConfiguration> endPoints)
        {
            if (endPoints.Any(x => string.IsNullOrEmpty(x.Name)))
                throw new Exception("One or more EndPointConfigurations has a blank name.  Name is required for all EndPointConfigurations");

            if (endPoints.Any(x => string.IsNullOrEmpty(x.API_Name)))
                throw new Exception("One or more EndPointConfigurations has a blank API_Name.  API_Name is required for all EndPointConfigurations");

            var dupes = endPoints.GroupBy(x => x.Name).Where(x => x.Count() > 1);  // Must be unique across all api_names

            if (dupes.Any())
                throw new Exception($"Duplicate EndPointConfiguration found. EndPoint Name: {dupes.First().Key}." + Environment.NewLine + "Each EndPointConfiguration must have a unique name.  Set the IsActive flag to false to bypass an EndPointConfiguration.");
        }
    }
}
