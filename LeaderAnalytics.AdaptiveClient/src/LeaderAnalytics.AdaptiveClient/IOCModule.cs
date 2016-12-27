using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;


namespace LeaderAnalytics.AdaptiveClient
{
    public class IOCModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterGeneric(typeof(ServiceClient<>)).As(typeof(IServiceClient<>)).InstancePerLifetimeScope();
            builder.RegisterType<NetworkUtilities>().As<INetworkUtilities>();
            builder.RegisterType<InProcessEndPointValidator>().Keyed<IEndPointValidator>(EndPointType.InProcess);
            builder.RegisterType<HttpEndPointValidator>().Keyed<IEndPointValidator>(EndPointType.WCF);
            builder.RegisterType<HttpEndPointValidator>().Keyed<IEndPointValidator>(EndPointType.HTTP);

            builder.Register<Func<IEndPointConfiguration>>(c => { IComponentContext cxt = c.Resolve<IComponentContext>(); return () => cxt.Resolve<EndPointContext>().CurrentEndPoint; });
            builder.Register<Func<Type, IEndPointCollection>>(c => { IComponentContext cxt = c.Resolve<IComponentContext>(); return t => cxt.ResolveKeyed<IEndPointCollection>(t); });
            builder.Register<Func<EndPointType, IEndPointValidator>>(c => { IComponentContext cxt = c.Resolve<IComponentContext>(); return ep => cxt.ResolveKeyed<IEndPointValidator>(ep); });
            builder.RegisterType<EndPointContext>().InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(ClientResolver<>)).As(typeof(IClientResolver<>));
        }
    }
}
