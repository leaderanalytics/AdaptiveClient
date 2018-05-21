using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;

namespace LeaderAnalytics.AdaptiveClient
{
    public static class RegistrationHelperExtensions
    {
        /// <summary>
        /// Registers a collection of EndPointConfiguration objects.
        /// </summary>
        /// <param name="endPoints">Collection of EndPointConfiguration objects</param>
        public static RegistrationHelper RegisterEndPoints(this RegistrationHelper helper, IEnumerable<IEndPointConfiguration> endPoints)
        {
            if (endPoints == null)
                throw new ArgumentNullException("endPoints");

            // Do not register endpoints with the container.  A list of endpoints is available when an Perimeter is resolved.
            endPoints = endPoints.Where(x => x.IsActive);
            EndPointUtilities.ValidateEndPoints(endPoints);

            foreach (var perimeter in endPoints.GroupBy(x => x.API_Name))
                helper.EndPointDict.Add(perimeter.Key, new Perimeter(perimeter.Key, perimeter.ToList()));

            return helper;
        }

        /// <summary>
        /// Registers a service. Call RegisterEndPoints before calling this method.
        /// </summary>
        /// <typeparam name="TService">Concrete class that implements TInterface i.e. OrdersService</typeparam>
        /// <typeparam name="TInterface">Interface of service i.e. IOrdersService</typeparam>
        /// <param name="endPoint">IEndPointConfiguration with EndPointType, API_Name, and providerName to use as keys</param>
        public static RegistrationHelper RegisterService<TService, TInterface>(this RegistrationHelper helper, IEndPointConfiguration endPoint)
        {
            if (endPoint == null)
                throw new ArgumentNullException("ep");

            RegisterService<TService, TInterface>(helper, endPoint.EndPointType, endPoint.API_Name, endPoint.ProviderName);
            return helper;
        }

        /// <summary>
        /// Registers a service. Call RegisterEndPoints before calling this method.
        /// </summary>
        /// <typeparam name="TService">Concrete class that implements TInterface i.e. OrdersService</typeparam>
        /// <typeparam name="TInterface">Interface of service i.e. IOrdersService</typeparam>
        /// <param name="endPointType">Type of client that will access this service i.e. HTTP, InProcess, WCF</param>
        /// <param name="apiName">API_Name of EndPointConfiguration objects  TInterface</param>
        /// <param name="providerName">Similar to provider name in a connection string, describes technology provider i.e. MSSQL, MySQL, SQLNCLI, etc.</param>
        public static RegistrationHelper RegisterService<TService, TInterface>(this RegistrationHelper helper, string endPointType, string apiName, string providerName)
        {
            if (String.IsNullOrEmpty(endPointType))
                throw new ArgumentNullException("endPointType");
            if (string.IsNullOrEmpty(apiName))
                throw new ArgumentNullException("apiName");
            if (providerName == null)
                providerName = string.Empty;


            RegisterPerimeter(helper, typeof(TInterface), apiName);

            helper.Builder.Register<Func<string, string, TInterface>>(c => {
                IComponentContext cxt = c.Resolve<IComponentContext>();
                return (ept, pn) => new ResolutionHelper(cxt).ResolveClient<TInterface>(ept, pn);
            });
            helper.Builder.RegisterType<TService>().Keyed<TInterface>(endPointType + providerName);
            return helper;
        }

        /// <summary>
        /// Registers a validator for a given EndPointType.  A validator is used to determine if an EndPoint is alive and able to handle requests.
        /// </summary>
        /// <typeparam name="TValidator">The implementation of IEndPointValidator that will handle validation requests for the specified EndPointType</typeparam>
        /// <param name="endPointType">The type of EndPoint that will be validated by the specified implementation of IEndPointValidator</param>
        /// <returns></returns>
        public static RegistrationHelper RegisterEndPointValidator<TValidator>(this RegistrationHelper helper, string endPointType, string providerName) where TValidator : IEndPointValidator
        {
            if (String.IsNullOrEmpty(endPointType))
                throw new ArgumentNullException("endPointType");
            if (string.IsNullOrEmpty(providerName))
                throw new ArgumentNullException("providerName");

            helper.Builder.RegisterType<TValidator>().Keyed<IEndPointValidator>(endPointType + providerName);
            return helper;
        }

        

        /// <summary>
        /// Registers an Action that accepts logging messages.
        /// </summary>
        /// <param name="logger"></param>
        public static RegistrationHelper RegisterLogger(this RegistrationHelper helper, Action<string> logger)
        {
            helper.Builder.RegisterInstance<Action<string>>(logger);
            return helper;
        }

        public static RegistrationHelper RegisterModule(this RegistrationHelper helper, params IAdaptiveClientModule[] modules)
        {
            if (!modules?.Any() ?? false)
                return helper;

            foreach (IAdaptiveClientModule module in modules)
                module.Register(helper);

            return helper;
        }

        /// <summary>
        /// Register Perimeter by name using service interface as key. 
        /// </summary>
        /// <param name="serviceInterface">Type of interface i.e. IUsersService</param>
        /// <param name="api_name">Name of API that implements passed service interface</param>
        public static void RegisterPerimeter(this RegistrationHelper helper, Type serviceInterface, string api_name)
        {
            IPerimeter perimeter;
            helper.EndPointDict.TryGetValue(api_name, out perimeter);

            if (perimeter == null)
                throw new Exception($"A Perimeter named {api_name} was not found.  Call RegisterEndPoints before calling Register.");

            string existingAPIRegistration = null;
            helper.ServiceRegistrations.TryGetValue(serviceInterface.FullName, out existingAPIRegistration);

            if(existingAPIRegistration != null && existingAPIRegistration != api_name)
                throw new Exception($"A service type can be registered with only one API.  Type {serviceInterface.Name} has been registered with an API named {existingAPIRegistration}.  A second attempt to register the same type with API {api_name} is being made.");
            else if(existingAPIRegistration == null)
                helper.ServiceRegistrations.Add(serviceInterface.FullName, api_name);

            helper.Builder.RegisterInstance<IPerimeter>(perimeter).Keyed<IPerimeter>(serviceInterface);
        }
    }
}
