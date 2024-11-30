namespace LeaderAnalytics.AdaptiveClient.Tests;

[TestFixture]
public class TryTests : BaseTest
{
    [Test]
    public void Reslove_InProcessClient_of_type_IDummyAPI1()
    {
        Moq.Mock<IEndPointValidator> MSSQL_Validator_Mock = new();
        MSSQL_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(true);
        Moq.Mock<IEndPointValidator> HTTP_Validator_Mock = new();
        HTTP_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(false);
        builder.RegisterInstance(MSSQL_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(HTTP_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.HTTP + ProviderName.HTTP);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(api => api.GetString(), null);
        Assert.That("Application_SQL1", Is.EqualTo(client1.CurrentEndPoint.Name));
        Assert.That("InProcessClient1", Is.EqualTo(result));
    }
    

    [Test]
    public void Reslove_InProcessClientAsync_of_type_IDummyAPI1()
    {
        Moq.Mock<IEndPointValidator> MSSQL_Validator_Mock = new();
        MSSQL_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(true);
        Moq.Mock<IEndPointValidator> HTTP_Validator_Mock = new();
        HTTP_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(false);
        builder.RegisterInstance(MSSQL_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(HTTP_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.HTTP + ProviderName.HTTP);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(async api => await api.GetStringAsync(), null).Result;
        Assert.That("Application_SQL1", Is.EqualTo(client1.CurrentEndPoint.Name));
        Assert.That("InProcessClient1", Is.EqualTo(result));
    }
    
    [Test]
    public void Reslove_WebAPIClient_of_type_IDummyAPI1()
    {
        Moq.Mock<IDummyAPI1> inProcessClientMock = new();
        inProcessClientMock.Setup(x => x.GetString()).Throws(new Exception("OMG"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;

        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(x => x.GetString(), null);
        Assert.That("WebAPIClient1", Is.EqualTo(result));
    }

    
    [Test]
    public void Uses_cached_endpoint_on_second_call()
    {
        int inProcessCalls = 0;

        Moq.Mock<IDummyAPI1> inProcessClientMock = new();
        inProcessClientMock.Setup(x => x.GetString()).Callback(() => inProcessCalls++).Throws(new Exception("OMG"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;

        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(x => x.GetString());
        Assert.That("WebAPIClient1", Is.EqualTo(result));
        Assert.That(2, Is.EqualTo(inProcessCalls));

        // do it again and use the cached endpoint:

        IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result2 = client2.Try(x => x.GetString());
        Assert.That("WebAPIClient1", Is.EqualTo(result));
        Assert.That(2, Is.EqualTo(inProcessCalls));
    }
    
    [Test]
    public void Uses_cached_endpoint_on_third_call_after_override_on_second_call()
    {
        int inProcessCalls = 0;

        Moq.Mock<IDummyAPI1> inProcessClientMock = new();
        inProcessClientMock.Setup(x => x.GetString()).Callback(() => inProcessCalls++).Throws(new Exception("OMG"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;

        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(x => x.GetString());
        Assert.That("WebAPIClient1", Is.EqualTo(result));
        Assert.That(2, Is.EqualTo(inProcessCalls));

        //  use the Application_WebAPI1A EndPoint by passing it to the client as an override

        LogMessages = null;
        result = client1.Try(x => x.GetString(), "Application_WebAPI1A");
        Assert.That("WebAPIClient1", Is.EqualTo(result));
        Assert.That(client1.CurrentEndPoint.Name, Is.EqualTo("Application_WebAPI1A"));


        // do it again and use the cached endpoint which is still Application_WebAPI1A:

        IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result2 = client2.Try(x => x.GetString());
        Assert.That("WebAPIClient1", Is.EqualTo(result));
        Assert.That(client1.CurrentEndPoint.Name, Is.EqualTo("Application_WebAPI1A"));
        Assert.That(2, Is.EqualTo(inProcessCalls));
    }

    
    [Test]
    public void Uses_next_endpoint_when_cached_endpoint_fails_on_second_call()
    {
        int inProcessCalls = 0;
        int webAPICalls = 0;

        Moq.Mock<IDummyAPI1> inProcessClientMock = new();
        inProcessClientMock.Setup(x => x.GetString()).Callback(() => inProcessCalls++).Throws(new Exception("OMG"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);

        Moq.Mock<IDummyAPI1> webAPIClientMock = new();
        webAPIClientMock.Setup(x => x.GetString()).Callback(() => { if(webAPICalls++ == 1) throw new Exception("OMG"); }).Returns("WebAPIClient1");
        IDummyAPI1 webAPIClient = webAPIClientMock.Object;
        builder.RegisterInstance(webAPIClient).Keyed<IDummyAPI1>(EndPointType.HTTP + ProviderName.HTTP);

        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(x => x.GetString());
        Assert.That("WebAPIClient1", Is.EqualTo(result));
        Assert.That(2, Is.EqualTo(inProcessCalls));
        Assert.That(1, Is.EqualTo(webAPICalls));

        // Application_WebAPI1 is the cached EndPoint.  We expect it to fail on this call and we use Application_WebAPI1A

        result = client1.Try(x => x.GetString());
        Assert.That("WebAPIClient1", Is.EqualTo(result));
        Assert.That("Application_WebAPI1A", Is.EqualTo(client1.CurrentEndPoint.Name));
        Assert.That(3, Is.EqualTo(webAPICalls));

        // do it again and use the cached endpoint which is still Application_WebAPI1A:

        IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result2 = client2.Try(x => x.GetString());
        Assert.That("WebAPIClient1", Is.EqualTo(result));
        Assert.That(client1.CurrentEndPoint.Name, Is.EqualTo("Application_WebAPI1A"));
        Assert.That(2, Is.EqualTo(inProcessCalls));
    }

              
    [Test]
    public void Throws_when_invalid_EndPoint_name_is_passed_as_override()
    {
        int inProcessCalls = 0;

        Moq.Mock<IDummyAPI1> inProcessClientMock = new();
        inProcessClientMock.Setup(x => x.GetString()).Callback(() => inProcessCalls++).Throws(new Exception("OMG"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;

        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(x => x.GetString());
        Assert.That("WebAPIClient1", Is.EqualTo(result));
        Assert.That(2, Is.EqualTo(inProcessCalls));

        // cached EndPoint is set but we are going to pass an invalid name

        IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        Assert.Throws<Exception>(() => client2.Try(x => x.GetString(), "does not exist"));
    }

    
    [Test]
    public void Inner_exceptions_are_maintained_for_each_client_call()
    {
        // We catch and log the error if a client throws - however we don't propagate errors unless
        // we run out of endpoints to try.  This test asserts that errors from each client are maintained 
        // and propagated as inner exceptions when we exhaust all endpoints.

        Moq.Mock<IDummyAPI1> inProcessClientMock = new();
        inProcessClientMock.Setup(x => x.GetString()).Throws(new Exception("InProcess Exception"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);

        Moq.Mock<IDummyAPI1> webAPIClientMock = new();
        webAPIClientMock.Setup(x => x.GetString()).Throws(new Exception("WebAPI Exception"));
        IDummyAPI1 webAPIClient = webAPIClientMock.Object;
        builder.RegisterInstance(webAPIClient).Keyed<IDummyAPI1>(EndPointType.HTTP + ProviderName.HTTP);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        Exception ex = Assert.Throws<Exception>(() => client1.Try(x => x.GetString()));
        Assert.That(4, Is.EqualTo(((AggregateException)(ex.InnerException)).InnerExceptions.Count));
        
        // Assert that the error thrown by the client is maintained
        Assert.That("InProcess Exception", Is.EqualTo(((AggregateException)(ex.InnerException)).InnerExceptions[0].InnerException.Message));
        Assert.That("InProcess Exception", Is.EqualTo(((AggregateException)(ex.InnerException)).InnerExceptions[1].InnerException.Message));
        Assert.That("WebAPI Exception", Is.EqualTo(((AggregateException)(ex.InnerException)).InnerExceptions[2].InnerException.Message));
        Assert.That("WebAPI Exception", Is.EqualTo(((AggregateException)(ex.InnerException)).InnerExceptions[3].InnerException.Message));
    }

    
    [Test]
    public void Inner_exceptions_are_maintained_for_each_client_call_async()
    {
        // We catch and log the error if a client throws - however we don't propagate errors unless
        // we run out of endpoints to try.  This test asserts that errors from each client are maintained 
        // and propagated as inner exceptions when we exhaust all endpoints.
        
        Moq.Mock<IDummyAPI1> inProcessClientMock = new();
        inProcessClientMock.Setup(x => x.GetString()).Throws(new Exception("InProcess Exception"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);

        Moq.Mock<IDummyAPI1> webAPIClientMock = new();
        webAPIClientMock.Setup(x => x.GetString()).Throws(new Exception("WebAPI Exception"));
        IDummyAPI1 webAPIClient = webAPIClientMock.Object;
        builder.RegisterInstance(webAPIClient).Keyed<IDummyAPI1>(EndPointType.HTTP + ProviderName.HTTP);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        Exception ex = Assert.ThrowsAsync<Exception>(async () => await client1.TryAsync(async x => { await Task.Delay(0); x.GetString(); }));
        
        Assert.That(4, Is.EqualTo(((AggregateException)(ex.InnerException)).InnerExceptions.Count));

        // Assert that the error thrown by the client is maintained
        Assert.That("InProcess Exception", Is.EqualTo(((AggregateException)(ex.InnerException)).InnerExceptions[0].InnerException.Message));
        Assert.That("InProcess Exception", Is.EqualTo(((AggregateException)(ex.InnerException)).InnerExceptions[1].InnerException.Message));
        Assert.That("WebAPI Exception", Is.EqualTo(((AggregateException)(ex.InnerException)).InnerExceptions[2].InnerException.Message));
        Assert.That("WebAPI Exception", Is.EqualTo(((AggregateException)(ex.InnerException)).InnerExceptions[3].InnerException.Message));
        
    }
}
