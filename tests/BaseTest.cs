namespace LeaderAnalytics.AdaptiveClient.Tests;

public class BaseTest
{
    public ContainerBuilder builder;
    public RegistrationHelper registrationHelper;
    public List<string> LogMessages;
    public List<IEndPointConfiguration> EndPoints;

    public BaseTest()
    {
    
    }

    [SetUp]
    public void Setup()
    {
        LogMessages = new List<string>();
        builder = new ContainerBuilder();
        EndPoints = EndPointUtilities.LoadEndPoints("appsettings.json").ToList();
        registrationHelper = new RegistrationHelper(builder);
        builder.RegisterModule(new AutofacModule());
        registrationHelper.RegisterModule(new AdaptiveClientModule(EndPoints, msg => this.LogMessages.Add(msg)));
    }
}

public class AdaptiveClientModule : IAdaptiveClientModule
{
    private List<IEndPointConfiguration> EndPoints;
    private Action<string> Logger;

    public AdaptiveClientModule(List<IEndPointConfiguration> endPoints, Action<string> logger)
    {
        EndPoints = endPoints;
        Logger = logger;
    }

    public void Register(RegistrationHelper registrationHelper)
    {
        registrationHelper
            .RegisterEndPoints(EndPoints)
            .RegisterService<InProcessClient1, IDummyAPI1>(EndPointType.InProcess, APINames.DummyAPI1, ProviderName.MSSQL)
            .RegisterService<InProcessClient1, IDummyAPI1>(EndPointType.InProcess, APINames.DummyAPI1,ProviderName.MySQL)
            .RegisterService<InProcessClient3, IDummyAPI1>(EndPointType.InProcess, APINames.DummyAPI1, ProviderName.MySQL)
            .RegisterService<WebAPIClient1, IDummyAPI1>(EndPointType.HTTP, APINames.DummyAPI1, ProviderName.HTTP)

            .RegisterService<InProcessClient2, IDummyAPI2>(EndPointType.InProcess, APINames.DummyAPI2, ProviderName.MSSQL)
            .RegisterService<WebAPIClient2, IDummyAPI2>(EndPointType.HTTP, APINames.DummyAPI2, ProviderName.HTTP)
            
            .RegisterEndPointValidator<MSSQL_EndPointValidator>(EndPointType.InProcess, ProviderName.MSSQL)
            .RegisterEndPointValidator<MySQL_EndPointValidator>(EndPointType.InProcess, ProviderName.MySQL)
            .RegisterEndPointValidator<Http_EndPointValidator>(EndPointType.WCF, ProviderName.HTTP)
            .RegisterEndPointValidator<Http_EndPointValidator>(EndPointType.HTTP, ProviderName.HTTP)
            .RegisterLogger(Logger);
    }
}
