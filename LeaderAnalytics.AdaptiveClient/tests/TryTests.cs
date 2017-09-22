using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaderAnalytics.AdaptiveClient;
using NUnit.Framework;
using Autofac;
using Moq;

namespace LeaderAnalytics.AdaptiveClient.Tests
{
    [TestFixture]
    public class TryTests : BaseTest
    {
        [Test]
        public void Reslove_InProcessClient_of_type_IDummyAPI1()
        {
            Moq.Mock<INetworkUtilities> networkUtilMock = new Mock<INetworkUtilities>();
            networkUtilMock.Setup(x => x.VerifyDBServerConnectivity(Moq.It.IsAny<string>())).Returns(true);
            networkUtilMock.Setup(x => x.VerifyHttpServerAvailability(Moq.It.IsAny<string>())).Returns(false);
            INetworkUtilities networkUtil = networkUtilMock.Object;
            builder.RegisterInstance(networkUtil).As<INetworkUtilities>();
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Try(api => api.GetString(), null);
            Assert.AreEqual("Application_SQL1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("InProcessClient1", result);
        }


        [Test]
        public void Reslove_InProcessClientAsync_of_type_IDummyAPI1()
        {
            Moq.Mock<INetworkUtilities> networkUtilMock = new Mock<INetworkUtilities>();
            networkUtilMock.Setup(x => x.VerifyDBServerConnectivity(Moq.It.IsAny<string>())).Returns(true);
            networkUtilMock.Setup(x => x.VerifyHttpServerAvailability(Moq.It.IsAny<string>())).Returns(false);
            INetworkUtilities networkUtil = networkUtilMock.Object;
            builder.RegisterInstance(networkUtil).As<INetworkUtilities>();
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

            builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess);
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

            builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess);
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Try(x => x.GetString());
            Assert.AreEqual("WebAPIClient1", result);
            Assert.AreEqual(1, inProcessCalls);

            // do it again and use the cached endpoint:

            IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result2 = client2.Try(x => x.GetString());
            Assert.AreEqual("WebAPIClient1", result);
            Assert.AreEqual(1, inProcessCalls);
        }


        [Test]
        public void Uses_cached_endpoint_on_third_call_after_override_on_second_call()
        {
            int inProcessCalls = 0;

            Moq.Mock<IDummyAPI1> inProcessClientMock = new Mock<IDummyAPI1>();
            inProcessClientMock.Setup(x => x.GetString()).Callback(() => inProcessCalls++).Throws(new Exception("OMG"));
            IDummyAPI1 inProcessClient = inProcessClientMock.Object;

            builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess);
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Try(x => x.GetString());
            Assert.AreEqual("WebAPIClient1", result);
            Assert.AreEqual(1, inProcessCalls);

            //  use the Application_WebAPI1A EndPoint by passing it to the client as an override

            LogMessage = null;
            result = client1.Try(x => x.GetString(), "Application_WebAPI1A");
            Assert.AreEqual("WebAPIClient1", result);
            Assert.AreEqual(client1.CurrentEndPoint.Name, "Application_WebAPI1A");


            // do it again and use the cached endpoint which is still Application_WebAPI1A:

            IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result2 = client2.Try(x => x.GetString());
            Assert.AreEqual("WebAPIClient1", result);
            Assert.AreEqual(client1.CurrentEndPoint.Name, "Application_WebAPI1A");
            Assert.AreEqual(1, inProcessCalls);
        }


        [Test]
        public void Uses_next_endpoint_when_cached_endpoint_fails_on_second_call()
        {
            int inProcessCalls = 0;
            int webAPICalls = 0;

            Moq.Mock<IDummyAPI1> inProcessClientMock = new Mock<IDummyAPI1>();
            inProcessClientMock.Setup(x => x.GetString()).Callback(() => inProcessCalls++).Throws(new Exception("OMG"));
            IDummyAPI1 inProcessClient = inProcessClientMock.Object;
            builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess);

            Moq.Mock<IDummyAPI1> webAPIClientMock = new Mock<IDummyAPI1>();
            webAPIClientMock.Setup(x => x.GetString()).Callback(() => { if(webAPICalls++ == 1) throw new Exception("OMG"); }).Returns("WebAPIClient1");
            IDummyAPI1 webAPIClient = webAPIClientMock.Object;
            builder.RegisterInstance(webAPIClient).Keyed<IDummyAPI1>(EndPointType.HTTP);

            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Try(x => x.GetString());
            Assert.AreEqual("WebAPIClient1", result);
            Assert.AreEqual(1, inProcessCalls);
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
            Assert.AreEqual(1, inProcessCalls);
        }


        [Test]
        public void Throws_when_invalid_EndPoint_name_is_passed_as_override()
        {
            int inProcessCalls = 0;

            Moq.Mock<IDummyAPI1> inProcessClientMock = new Mock<IDummyAPI1>();
            inProcessClientMock.Setup(x => x.GetString()).Callback(() => inProcessCalls++).Throws(new Exception("OMG"));
            IDummyAPI1 inProcessClient = inProcessClientMock.Object;

            builder.RegisterInstance(inProcessClient).Keyed<IDummyAPI1>(EndPointType.InProcess);
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Try(x => x.GetString());
            Assert.AreEqual("WebAPIClient1", result);
            Assert.AreEqual(1, inProcessCalls);

            // cached EndPoint is set but we are going to pass an invalid name

            IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            Assert.Throws<Exception>(() => client2.Try(x => x.GetString(), "does not exist"));
        }
    }
}
