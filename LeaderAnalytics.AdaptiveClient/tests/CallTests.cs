using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using NUnit;
//using NUnit.Framework;
using LeaderAnalytics.AdaptiveClient;
using Autofac;
using Autofac.Core;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeaderAnalytics.AdaptiveClient.Tests
{
    [TestClass]
    public class CallTests : BaseTest
    {
        interface IDummy3 :IDisposable { string GetString(); } // Not registered

        [TestMethod]
        public void Reslove_InProcessClient_of_type_IDummyAPI1()
        {
            Moq.Mock<INetworkUtilities> networkUtilMock = new Mock<INetworkUtilities>();
            networkUtilMock.Setup(x => x.VerifyDBServerConnectivity(Moq.It.IsAny<string>())).Returns(true);
            networkUtilMock.Setup(x => x.VerifyHttpServerAvailability(Moq.It.IsAny<string>())).Returns(false);
            INetworkUtilities networkUtil = networkUtilMock.Object;
            builder.RegisterInstance(networkUtil).As<INetworkUtilities>();
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Call(x => x.GetString());
            Assert.AreEqual("Application_SQL1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("InProcessClient1", result);
        }

        [TestMethod]
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
            Assert.AreEqual("Application_WebAPI1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("WebAPIClient1", result);
            Assert.AreEqual("Failed to connect to EndPoint named Application_SQL1 when resolving a client of type LeaderAnalytics.AdaptiveClient.Tests.IDummyAPI1.", LogMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(DependencyResolutionException),"An error occurred during the activation of a particular registration. See the inner exception for details. Registration: Activator = AdaptiveClient`1 (ReflectionActivator), Services = [LeaderAnalytics.AdaptiveClient.IAdaptiveClient`1[[LeaderAnalytics.AdaptiveClient.Tests.CallTests+IDummy3, LeaderAnalytics.AdaptiveClient.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]], Lifetime = Autofac.Core.Lifetime.CurrentScopeLifetime, Sharing = Shared, Ownership = OwnedByLifetimeScope ---> An exception was thrown while invoking the constructor 'Void .ctor(Autofac.ILifetimeScope, System.Func`2[System.Type,LeaderAnalytics.AdaptiveClient.IPerimeter], LeaderAnalytics.AdaptiveClient.EndPointCache)' on type 'AdaptiveClient`1'. ---> Interface IDummy3 has not been registered.  Use RegistrationHelper to register IDummy3 with one or more implementations, EndPointConfiguration, and API names. (See inner exception for details.) (See inner exception for details.)")]
        public void Throws_when_resolving_unregistered_client()
        {
            Moq.Mock<INetworkUtilities> networkUtilMock = new Mock<INetworkUtilities>();
            networkUtilMock.Setup(x => x.VerifyDBServerConnectivity(Moq.It.IsAny<string>())).Returns(false);
            networkUtilMock.Setup(x => x.VerifyHttpServerAvailability(Moq.It.IsAny<string>())).Returns(true);
            INetworkUtilities networkUtil = networkUtilMock.Object;
            builder.RegisterInstance(networkUtil).As<INetworkUtilities>();
            IContainer container = builder.Build();
            IAdaptiveClient<IDummy3> client1 = container.Resolve<IAdaptiveClient<IDummy3>>();
        }

        [TestMethod]
        public void Uses_cached_endpoint_on_second_call()
        {
            int inProcessCalls = 0;
            int webAPICalls = 0;


            // NetworkUtilities Mock
            Moq.Mock<INetworkUtilities> networkUtilMock = new Mock<INetworkUtilities>();
            networkUtilMock.Setup(x => x.VerifyDBServerConnectivity(Moq.It.IsAny<string>())).Callback(() => inProcessCalls++).Returns(false);
            networkUtilMock.Setup(x => x.VerifyHttpServerAvailability(Moq.It.IsAny<string>())).Callback(() => webAPICalls++).Returns(true);
            INetworkUtilities networkUtil = networkUtilMock.Object;
            builder.RegisterInstance(networkUtil).As<INetworkUtilities>();
            IContainer container = builder.Build();

            IAdaptiveClient<IDummyAPI1> client1 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result = client1.Call(x => x.GetString());
            Assert.AreEqual("Application_WebAPI1", client1.CurrentEndPoint.Name);
            Assert.AreEqual("WebAPIClient1", result);
            Assert.AreEqual(1, inProcessCalls);
            Assert.AreEqual(1, webAPICalls);
            
            // do it again and use the cached endpoint:

            IAdaptiveClient<IDummyAPI1> client2 = container.Resolve<IAdaptiveClient<IDummyAPI1>>();
            string result2 = client2.Call(x => x.GetString());
            Assert.AreEqual("Application_WebAPI1", client2.CurrentEndPoint.Name);
            Assert.AreEqual("WebAPIClient1", result2);
            Assert.AreEqual(1, inProcessCalls);   // We should not test the in process endpoint again - we go directly to the cached HTTP endpoint.
            Assert.AreEqual(1, webAPICalls);
        }
    }
}
