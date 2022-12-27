namespace LeaderAnalytics.AdaptiveClient;

public class AutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.RegisterGeneric(typeof(AdaptiveClient<>)).As(typeof(IAdaptiveClient<>)).InstancePerLifetimeScope();
        builder.Register<Func<IEndPointConfiguration>>(c => { IComponentContext cxt = c.Resolve<IComponentContext>(); return () => cxt.Resolve<EndPointContext>().CurrentEndPoint; });
        builder.Register<Func<Type, IPerimeter>>(c => { ILifetimeScope cxt = c.Resolve<ILifetimeScope>(); return t => new ResolutionHelper(cxt).ResolvePerimeter(t); });
        builder.Register<Func<string, string, IEndPointValidator>>(c => { ILifetimeScope cxt = c.Resolve<ILifetimeScope>(); return (eptype, providerName) => new ResolutionHelper(cxt).ResolveValidator(eptype, providerName); });
        builder.RegisterType<EndPointContext>().InstancePerLifetimeScope();     // per lifetimescope - see notes in EndPointContext.cs
        builder.RegisterType<EndPointCache>().SingleInstance();                 // singleton
        builder.RegisterGeneric(typeof(ClientFactory<>)).As(typeof(IClientFactory<>));
        builder.RegisterGeneric(typeof(ClientEvaluator<>)).As(typeof(IClientEvaluator<>));
        builder.RegisterInstance<Action<string>>(msg => { }); // default logger.  User can override by calling RegistrationHelper.RegisterLogger
        builder.RegisterType<ResolutionHelper>();
        builder.RegisterType<RegistrationHelper>();
    }
}

