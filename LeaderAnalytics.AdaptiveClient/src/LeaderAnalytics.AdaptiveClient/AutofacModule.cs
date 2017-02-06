using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using LeaderAnalytics.AdaptiveClient.Autofac;
//-
namespace LeaderAnalytics.AdaptiveClient
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterGeneric(typeof(AdaptiveClient<>)).As(typeof(IAdaptiveClient<>)).InstancePerLifetimeScope();
            builder.RegisterType<NetworkUtilities>().As<INetworkUtilities>();
            builder.RegisterType<InProcessEndPointValidator>().Keyed<IEndPointValidator>(EndPointType.InProcess);
            builder.RegisterType<HttpEndPointValidator>().Keyed<IEndPointValidator>(EndPointType.WCF);
            builder.RegisterType<HttpEndPointValidator>().Keyed<IEndPointValidator>(EndPointType.WebAPI);

            builder.Register<Func<IEndPointConfiguration>>(c => { IComponentContext cxt = c.Resolve<IComponentContext>(); return () => cxt.Resolve<EndPointContext>().CurrentEndPoint; });
            builder.Register<Func<Type, IPerimeter>>(c => { IComponentContext cxt = c.Resolve<IComponentContext>(); return t => cxt.ResolveKeyed<IPerimeter>(t); });
            builder.Register<Func<EndPointType, IEndPointValidator>>(c => { IComponentContext cxt = c.Resolve<IComponentContext>(); return ep => cxt.ResolveKeyed<IEndPointValidator>(ep); });
            builder.RegisterType<EndPointContext>().InstancePerLifetimeScope();     // per lifetimescope - see notes in EndPointContext.cs
            builder.RegisterType<EndPointCache>().SingleInstance();                 // singleton
            builder.RegisterGeneric(typeof(ClientFactory<>)).As(typeof(IClientFactory<>));
            builder.RegisterGeneric(typeof(ClientEvaluator<>)).As(typeof(IClientEvaluator<>));
        }
    }
}

