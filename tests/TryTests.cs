namespace LeaderAnalytics.AdaptiveClient.Tests;

[TestFixture]
public class TryTests : BaseTest
{
    [Test]
    public void Reslove_InProcessClient_of_type_IDummyAPI1()
    {
        Moq.Mock<IEndPointValidator> MSSQL_Validator_Mock = new Mock<IEndPointValidator>();
        MSSQL_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(true);
        Moq.Mock<IEndPointValidator> HTTP_Validator_Mock = new Mock<IEndPointValidator>();
        HTTP_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(false);
        builder.RegisterInstance(MSSQL_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(HTTP_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.HTTP + ProviderName.HTTP);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(api => api.GetString(), null);
        Assert.AreEqual("Application_SQL1", client1.CurrentEndPoint.Name);
        Assert.AreEqual("InProcessClient1", result);
    }
    

    [Test]
    public void Reslove_InProcessClientAsync_of_type_IDummyAPI1()
    {
        Moq.Mock<IEndPointValidator> MSSQL_Validator_Mock = new Mock<IEndPointValidator>();
        MSSQL_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(true);
        Moq.Mock<IEndPointValidator> HTTP_Validator_Mock = new Mock<IEndPointValidator>();
        HTTP_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(false);
        builder.RegisterInstance(MSSQL_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(HTTP_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.HTTP + ProviderName.HTTP);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(async api => await api.GetStringAsync(), null).Result;
        Assert.AreEqual("Application_SQL1", client1.CurrentEndPoint.Name);
        Assert.AreEqual("InProcessClient1", result);
    }
    
    [Test]
    public void Reslove_WebAPIClient_of_type_IDummyAPI1()
    {
        Moq.Mock<IDummyAPI1> inProcessClientMock = new Mock<IDummyAPI1>();
        inProcessClientMock.Setup(x => x.GetString()).Throws(new Exception("OMG"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;

        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(x => x.GetString(), null);
        Assert.AreEqual("WebAPIClient1", result);
    }

    
    [Test]
    public void Uses_cached_endpoint_on_second_call()
    {
        int inProcessCalls = 0;

        Moq.Mock<IDummyAPI1> inProcessClientMock = new Mock<IDummyAPI1>();
        inProcessClientMock.Setup(x => x.GetString()).Callback(() => inProcessCalls++).Throws(new Exception("OMG"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;

        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(x => x.GetString());
        Assert.AreEqual("WebAPIClient1", result);
        Assert.AreEqual(2, inProcessCalls);

        // do it again and use the cached endpoint:

        IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result2 = client2.Try(x => x.GetString());
        Assert.AreEqual("WebAPIClient1", result);
        Assert.AreEqual(2, inProcessCalls);
    }
    
    [Test]
    public void Uses_cached_endpoint_on_third_call_after_override_on_second_call()
    {
        int inProcessCalls = 0;

        Moq.Mock<IDummyAPI1> inProcessClientMock = new Mock<IDummyAPI1>();
        inProcessClientMock.Setup(x => x.GetString()).Callback(() => inProcessCalls++).Throws(new Exception("OMG"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;

        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(x => x.GetString());
        Assert.AreEqual("WebAPIClient1", result);
        Assert.AreEqual(2, inProcessCalls);

        //  use the Application_WebAPI1A EndPoint by passing it to the client as an override

        LogMessages = null;
        result = client1.Try(x => x.GetString(), "Application_WebAPI1A");
        Assert.AreEqual("WebAPIClient1", result);
        Assert.AreEqual(client1.CurrentEndPoint.Name, "Application_WebAPI1A");


        // do it again and use the cached endpoint which is still Application_WebAPI1A:

        IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result2 = client2.Try(x => x.GetString());
        Assert.AreEqual("WebAPIClient1", result);
        Assert.AreEqual(client1.CurrentEndPoint.Name, "Application_WebAPI1A");
        Assert.AreEqual(2, inProcessCalls);
    }

    
    [Test]
    public void Uses_next_endpoint_when_cached_endpoint_fails_on_second_call()
    {
        int inProcessCalls = 0;
        int webAPICalls = 0;

        Moq.Mock<IDummyAPI1> inProcessClientMock = new Mock<IDummyAPI1>();
        inProcessClientMock.Setup(x => x.GetString()).Callback(() => inProcessCalls++).Throws(new Exception("OMG"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);

        Moq.Mock<IDummyAPI1> webAPIClientMock = new Mock<IDummyAPI1>();
        webAPIClientMock.Setup(x => x.GetString()).Callback(() => { if(webAPICalls++ == 1) throw new Exception("OMG"); }).Returns("WebAPIClient1");
        IDummyAPI1 webAPIClient = webAPIClientMock.Object;
        builder.RegisterInstance(webAPIClient).Keyed<IDummyAPI1>(EndPointType.HTTP + ProviderName.HTTP);

        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(x => x.GetString());
        Assert.AreEqual("WebAPIClient1", result);
        Assert.AreEqual(2, inProcessCalls);
        Assert.AreEqual(1, webAPICalls);

        // Application_WebAPI1 is the cached EndPoint.  We expect it to fail on this call and we use Application_WebAPI1A

        result = client1.Try(x => x.GetString());
        Assert.AreEqual("WebAPIClient1", result);
        Assert.AreEqual("Application_WebAPI1A", client1.CurrentEndPoint.Name);
        Assert.AreEqual(3, webAPICalls);

        // do it again and use the cached endpoint which is still Application_WebAPI1A:

        IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result2 = client2.Try(x => x.GetString());
        Assert.AreEqual("WebAPIClient1", result);
        Assert.AreEqual(client1.CurrentEndPoint.Name, "Application_WebAPI1A");
        Assert.AreEqual(2, inProcessCalls);
    }

              
    [Test]
    public void Throws_when_invalid_EndPoint_name_is_passed_as_override()
    {
        int inProcessCalls = 0;

        Moq.Mock<IDummyAPI1> inProcessClientMock = new Mock<IDummyAPI1>();
        inProcessClientMock.Setup(x => x.GetString()).Callback(() => inProcessCalls++).Throws(new Exception("OMG"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;

        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        string result = client1.Try(x => x.GetString());
        Assert.AreEqual("WebAPIClient1", result);
        Assert.AreEqual(2, inProcessCalls);

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

        Moq.Mock<IDummyAPI1> inProcessClientMock = new Mock<IDummyAPI1>();
        inProcessClientMock.Setup(x => x.GetString()).Throws(new Exception("InProcess Exception"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);

        Moq.Mock<IDummyAPI1> webAPIClientMock = new Mock<IDummyAPI1>();
        webAPIClientMock.Setup(x => x.GetString()).Throws(new Exception("WebAPI Exception"));
        IDummyAPI1 webAPIClient = webAPIClientMock.Object;
        builder.RegisterInstance(webAPIClient).Keyed<IDummyAPI1>(EndPointType.HTTP + ProviderName.HTTP);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        Exception ex = Assert.Throws<Exception>(() => client1.Try(x => x.GetString()));
        Assert.AreEqual(4, ((AggregateException)(ex.InnerException)).InnerExceptions.Count);
        
        // Assert that the error thrown by the client is maintained
        Assert.AreEqual("InProcess Exception", ((AggregateException)(ex.InnerException)).InnerExceptions[0].InnerException.Message);
        Assert.AreEqual("InProcess Exception", ((AggregateException)(ex.InnerException)).InnerExceptions[1].InnerException.Message);
        Assert.AreEqual("WebAPI Exception", ((AggregateException)(ex.InnerException)).InnerExceptions[2].InnerException.Message);
        Assert.AreEqual("WebAPI Exception", ((AggregateException)(ex.InnerException)).InnerExceptions[3].InnerException.Message);
    }

    
    [Test]
    public void Inner_exceptions_are_maintained_for_each_client_call_async()
    {
        // We catch and log the error if a client throws - however we don't propagate errors unless
        // we run out of endpoints to try.  This test asserts that errors from each client are maintained 
        // and propagated as inner exceptions when we exhaust all endpoints.
        
        Moq.Mock<IDummyAPI1> inProcessClientMock = new Mock<IDummyAPI1>();
        inProcessClientMock.Setup(x => x.GetString()).Throws(new Exception("InProcess Exception"));
        IDummyAPI1 inProcessClient = inProcessClientMock.Object;
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);
        builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MySQL);

        Moq.Mock<IDummyAPI1> webAPIClientMock = new Mock<IDummyAPI1>();
        webAPIClientMock.Setup(x => x.GetString()).Throws(new Exception("WebAPI Exception"));
        IDummyAPI1 webAPIClient = webAPIClientMock.Object;
        builder.RegisterInstance(webAPIClient).Keyed<IDummyAPI1>(EndPointType.HTTP + ProviderName.HTTP);
        IContainer container = builder.Build();

        IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
        Exception ex = Assert.ThrowsAsync<Exception>(async () => await client1.TryAsync(async x => { await Task.Delay(0); x.GetString(); }));
        
        Assert.AreEqual(4, ((AggregateException)(ex.InnerException)).InnerExceptions.Count);

        // Assert that the error thrown by the client is maintained
        Assert.AreEqual("InProcess Exception", ((AggregateException)(ex.InnerException)).InnerExceptions[0].InnerException.Message);
        Assert.AreEqual("InProcess Exception", ((AggregateException)(ex.InnerException)).InnerExceptions[1].InnerException.Message);
        Assert.AreEqual("WebAPI Exception", ((AggregateException)(ex.InnerException)).InnerExceptions[2].InnerException.Message);
        Assert.AreEqual("WebAPI Exception", ((AggregateException)(ex.InnerException)).InnerExceptions[3].InnerException.Message);
        
    }
}
