using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit;
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
            Moq.Mock<IDummyAPI1> webClientMock1 = new Mock<IDummyAPI1>();
            webClientMock1.Setup(x => x.GetString()).Throws(new Exception("OMG"));
            IDummyAPI1 webClient1 = webClientMock1.Object;

            builder.RegisterInstance(webClient1).Keyed<IDummyAPI1>(EndPointType.InProcess);
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = null;
            client1.Try(api => result = api.GetString(), null);
            Assert.AreEqual("WebAPIClient1", result);
            
            // try again and make sure we use CurrentEndPoint
            client1.Try(api => result = api.GetString(), null);
            Assert.AreEqual("Application_WebAPI1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("WebAPIClient1", result);
        }
    }
}
