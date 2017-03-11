using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace LeaderAnalytics.AdaptiveClient
{
    public static class ResolutionHelper
    {
        public static TInterface ResolveClient<TInterface>(IComponentContext cxt, string ept)
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

        public static IEndPointValidator ResolveValidator(IComponentContext cxt, string endPointType)
        {
            IEndPointValidator result = null;
            result = cxt.ResolveOptionalKeyed<IEndPointValidator>(endPointType);

            if (result == null)
                throw new ComponentNotRegisteredException($"An attempt to resolve an EndPointValidator for EndPointType { endPointType } failed.  Use the RegisterEndPointValidator method on the RegistrationHelper to register an EndPointValidator for each EndPointType.");

            return result;
        }
    }
}
