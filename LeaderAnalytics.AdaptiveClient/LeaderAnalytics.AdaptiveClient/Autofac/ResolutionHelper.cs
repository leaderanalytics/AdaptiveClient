using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Autofac;

namespace LeaderAnalytics.AdaptiveClient
{
    public static class ResolutionHelper
    {
        public static TInterface ResolveClient<TInterface>(IComponentContext cxt, EndPointType ept)
        {
            object result = null;
            cxt.TryResolveKeyed(ept, typeof(TInterface), out result); // Not an error if not resolved.
            return (TInterface)result;
        }


        public static IPerimeter ResolvePerimeter(IComponentContext cxt, Type typ)
        {
            IPerimeter result = null;
            result = cxt.ResolveOptionalKeyed<IPerimeter>(typ);

            if (result == null)
                throw new ComponentNotRegisteredException($"Interface { typ.Name } has not been registered.  Use RegistrationHelper to register { typ.Name } with one or more implementations, EndPointConfiguration, and API names.");

            return result;
        }

        public static ChannelFactory<TInterface> ResolveChannelFactory<TInterface>(IComponentContext cxt, string url)
        {
            IEndPointConfiguration ep = (cxt.Resolve<Func<IEndPointConfiguration>>())();
            // Todo: need to resolve ChannelFactory
            return new ChannelFactory<TInterface>(new BasicHttpBinding(), new EndpointAddress(ep.ConnectionString + url));
        }
    }
}
