using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using LeaderAnalytics.AdaptiveClient;
using NUnit.Framework;
using Autofac;
using Moq;


namespace LeaderAnalytics.AdaptiveClient.Tests
{
    [TestFixture]
    public class Class1 : BaseTest
    {
        [Test]
        public void Reslove_InProcessClient_of_type_IDummyAPI1()
        {
            //Moq.Mock<IDummyAPI1> webClientMock1 = new Mock<IDummyAPI1>();
            //webClientMock1.Setup(x => x.GetString()).Returns("Hello from web client");

            //Moq.Mock<IDummyAPI2> webClientMock2 = new Mock<IDummyAPI2>();
            //webClientMock2.Setup(x => x.GetInt()).Returns(1);

            //Moq.Mock<IDummyAPI1> inProcessMock1 = new Mock<IDummyAPI1>();
            //inProcessMock1.Setup(x => x.GetString()).Returns("Hello from In Process client");

            //Moq.Mock<IDummyAPI2> inProcessMock2 = new Mock<IDummyAPI2>();
            //inProcessMock2.Setup(x => x.GetInt()).Returns(2);

            //IDummyAPI1 webClient1 = webClientMock1.Object;
            //IDummyAPI2 webClient2 = webClientMock2.Object;
            //IDummyAPI1 inProcessClient1 = inProcessMock1.Object;
            //IDummyAPI2 inProcessClient2 = inProcessMock2.Object;

            Moq.Mock<INetworkUtilities> networkUtilMock = new Mock<INetworkUtilities>();
            networkUtilMock.Setup(x => x.VerifyDBServerConnectivity(Moq.It.IsAny<string>())).Returns(true);
            networkUtilMock.Setup(x => x.VerifyHttpServerAvailability(Moq.It.IsAny<string>())).Returns(false);
            INetworkUtilities networkUtil = networkUtilMock.Object;
            builder.RegisterInstance(networkUtil).As<INetworkUtilities>();
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Call(x => x.GetString());
            Assert.AreEqual("InProcessClient1", result);
        }


        [Test]
        public void Reslove_WebAPIClient_of_type_IDummyAPI1()
        {
            Moq.Mock<INetworkUtilities> networkUtilMock = new Mock<INetworkUtilities>();
            networkUtilMock.Setup(x => x.VerifyDBServerConnectivity(Moq.It.IsAny<string>())).Returns(false);
            networkUtilMock.Setup(x => x.VerifyHttpServerAvailability(Moq.It.IsAny<string>())).Returns(true);
            INetworkUtilities networkUtil = networkUtilMock.Object;
            builder.RegisterInstance(networkUtil).As<INetworkUtilities>();
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Call(x => x.GetString());
            Assert.AreEqual("WebAPIClient1", result);
        }
    }
}
