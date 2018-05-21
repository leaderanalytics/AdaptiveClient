using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using LeaderAnalytics.AdaptiveClient;
using LeaderAnalytics.AdaptiveClient.Utilities;
using Autofac;
using Autofac.Core;
using Moq;

namespace LeaderAnalytics.AdaptiveClient.Tests
{
    [TestFixture]
    public class CallTests : BaseTest
    {
        interface IDummy3 :IDisposable { string GetString(); } // Not registered

        [Test]
        public void Reslove_InProcessClient1_of_type_IDummyAPI1()
        {
            Moq.Mock<IEndPointValidator> MSSQL_Validator_Mock = new Mock<IEndPointValidator>();
            MSSQL_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(true);
            Moq.Mock<IEndPointValidator> HTTP_Validator_Mock = new Mock<IEndPointValidator>();
            HTTP_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(false);
            builder.RegisterInstance(MSSQL_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
            builder.RegisterInstance(HTTP_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.HTTP + ProviderName.HTTP);

            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Call(x => x.GetString());
            Assert.AreEqual("Application_SQL1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("InProcessClient1", result);
        }
        
        [Test]
        public void Reslove_InProcessClient3_of_type_IDummyAPI1()
        {
            Moq.Mock<IEndPointValidator> MSSQL_Validator_Mock = new Mock<IEndPointValidator>();
            MSSQL_Validator_Mock.Setup(x => x.IsInterfaceAlive(It.IsAny<IEndPointConfiguration>())).Returns(false);
            Moq.Mock<IEndPointValidator> MySQL_Validator_Mock = new Mock<IEndPointValidator>();
            MySQL_Validator_Mock.Setup(x => x.IsInterfaceAlive(It.Is<IEndPointConfiguration>(z => z == EndPoints.First(y => y.Name == "Application_MySQL1")))).Returns(true);
            Moq.Mock<IEndPointValidator> HTTP_Validator_Mock = new Mock<IEndPointValidator>();
            HTTP_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(false);
            builder.RegisterInstance(MSSQL_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
            builder.RegisterInstance(MySQL_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MySQL);
            builder.RegisterInstance(HTTP_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.HTTP + ProviderName.HTTP);
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Call(x => x.GetString());
            Assert.AreEqual("Application_MySQL1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("InProcessClient3", result);
        }

        
        [Test]
        public void Reslove_InProcessClient3_of_type_IDummyAPI1_when_EndPoint_name_is_passed()
        {
            Moq.Mock<IEndPointValidator> InProcess_Validator_Mock = new Mock<IEndPointValidator>();
            InProcess_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(true);
            Moq.Mock<IEndPointValidator> HTTP_Validator_Mock = new Mock<IEndPointValidator>();
            HTTP_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(false);
            builder.RegisterInstance(InProcess_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
            builder.RegisterInstance(InProcess_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MySQL);
            builder.RegisterInstance(HTTP_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.HTTP + ProviderName.HTTP);
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Call(x => x.GetString());
            Assert.AreEqual("Application_SQL1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("InProcessClient1", result);

            string result2 = client1.Call(x => x.GetString(), "Application_MySQL1");
            Assert.AreEqual("Application_MySQL1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("InProcessClient3", result2);
        }


        [Test]
        public void Reslove_WebAPIClient_of_type_IDummyAPI1()
        {
            Moq.Mock<IEndPointValidator> InProcess_Validator_Mock = new Mock<IEndPointValidator>();
            InProcess_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(false);
            Moq.Mock<IEndPointValidator> HTTP_Validator_Mock = new Mock<IEndPointValidator>();
            HTTP_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(true);
            builder.RegisterInstance(InProcess_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
            builder.RegisterInstance(InProcess_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MySQL);
            builder.RegisterInstance(HTTP_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.HTTP + ProviderName.HTTP);
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Call(x => x.GetString());
            Assert.AreEqual("Application_WebAPI1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("WebAPIClient1", result);
        }
        
        
        [Test]
        public void Throws_when_resolving_unregistered_client()
        {
            Moq.Mock<IEndPointValidator> InProcess_Validator_Mock = new Mock<IEndPointValidator>();
            InProcess_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(false);
            Moq.Mock<IEndPointValidator> HTTP_Validator_Mock = new Mock<IEndPointValidator>();
            HTTP_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(true);
            builder.RegisterInstance(InProcess_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
            builder.RegisterInstance(InProcess_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MySQL);
            builder.RegisterInstance(HTTP_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.HTTP + ProviderName.HTTP);
            IContainer container = builder.Build();

            Assert.Throws<DependencyResolutionException>(() => container.Resolve<IAdaptiveClient<IDummy3>>());
        }

        
        [Test]
        public void Uses_cached_endpoint_on_second_call()
        {
            int inProcessCalls = 0;
            int webAPICalls = 0;

            Moq.Mock<IEndPointValidator> InProcess_Validator_Mock = new Mock<IEndPointValidator>();
            InProcess_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Callback(() => inProcessCalls++).Returns(false);
            Moq.Mock<IEndPointValidator> HTTP_Validator_Mock = new Mock<IEndPointValidator>();
            HTTP_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Callback(() => webAPICalls++).Returns(true);
            builder.RegisterInstance(InProcess_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
            builder.RegisterInstance(InProcess_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MySQL);
            builder.RegisterInstance(HTTP_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.HTTP + ProviderName.HTTP);
            IContainer container = builder.Build();


            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Call(x => x.GetString());
            Assert.AreEqual("Application_WebAPI1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("WebAPIClient1", result);
            Assert.AreEqual(2, inProcessCalls);
            Assert.AreEqual(1, webAPICalls);
            
            // do it again and use the cached endpoint:

            IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result2 = client2.Call(x => x.GetString());
            Assert.AreEqual("Application_WebAPI1", client2.CurrentEndPoint.Name);
            Assert.AreEqual("WebAPIClient1", result2);
            Assert.AreEqual(2, inProcessCalls);   // We should not test the in process endpoint again - we go directly to the cached HTTP endpoint.
            Assert.AreEqual(1, webAPICalls);
        }

        

        [Test]
        public void Client_exception_is_propagated()
        {
            Moq.Mock<IEndPointValidator> InProcess_Validator_Mock = new Mock<IEndPointValidator>();
            InProcess_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(true);
            Moq.Mock<IEndPointValidator> HTTP_Validator_Mock = new Mock<IEndPointValidator>();
            HTTP_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(false);
            builder.RegisterInstance(InProcess_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
            builder.RegisterInstance(InProcess_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MySQL);
            builder.RegisterInstance(HTTP_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.HTTP + ProviderName.HTTP);


            Moq.Mock<IDummyAPI1> inProcessClientMock = new Mock<IDummyAPI1>();
            inProcessClientMock.Setup(x => x.GetString()).Throws(new Exception("InProcess Exception"));
            IDummyAPI1 inProcessClient = inProcessClientMock.Object;
            builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess + ProviderName.MSSQL);

            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            Exception ex = Assert.Throws<Exception>(() => client1.Call(x => x.GetString()));

            Assert.AreEqual("Application_SQL1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("InProcess Exception", ex.Message);
        }


        
        [Test]
        public async Task CurrentEndPoint_is_thread_safe()
        {
            Moq.Mock<IEndPointValidator> InProcess_Validator_Mock = new Mock<IEndPointValidator>();
            InProcess_Validator_Mock.Setup(x => x.IsInterfaceAlive(Moq.It.IsAny<IEndPointConfiguration>())).Returns(true);
            builder.RegisterInstance(InProcess_Validator_Mock.Object).Keyed<IEndPointValidator>(EndPointType.InProcess + ProviderName.MSSQL);
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            IAdaptiveClient<IDummyAPI2> client2 = container.Resolve<IAdaptiveClient<IDummyAPI2>>();
            string result1 = client1.Call(x => x.GetString());
            Assert.AreEqual("Application_SQL1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("InProcessClient1", result1);
            int result2 = client2.Call(x => x.GetInt());
            Assert.AreEqual("Application_SQL2", client2.CurrentEndPoint.Name);
            Assert.AreEqual(1, result2);

            Task t1 = Task.Run(() => {

                for (int i = 0; i < 1000; i++)
                {
                    string r1 = client1.Call(x => x.GetString());
                    Assert.AreEqual("Application_SQL1", client1.CurrentEndPoint.Name);
                    Assert.AreEqual("InProcessClient1", r1);

                    int r2 = client2.Call(x => x.GetInt());
                    Assert.AreEqual("Application_SQL2", client2.CurrentEndPoint.Name);
                    Assert.AreEqual(1, r2);
                }
            });

            Task t2 = Task.Run(() => {

                for (int i = 0; i < 1000; i++)
                {
                    int r2 = client2.Call(x => x.GetInt());
                    Assert.AreEqual("Application_SQL2", client2.CurrentEndPoint.Name);
                    Assert.AreEqual(1, r2);

                    string r1 = client1.Call(x => x.GetString());
                    Assert.AreEqual("Application_SQL1", client1.CurrentEndPoint.Name);
                    Assert.AreEqual("InProcessClient1", r1);
                }
            });

            try
            {
                await Task.WhenAll(t1, t2);
            }
            catch (Exception)
            {
                throw;
            }
            
        }
    }
}
