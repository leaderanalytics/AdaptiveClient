using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Autofac;
using NUnit.Framework;
using LeaderAnalytics.AdaptiveClient;


namespace LeaderAnalytics.AdaptiveClient.Tests
{
    public class BaseTest
    {
        public ContainerBuilder builder;
        public AutofacRegistrationHelper registrationHelper;
        public List<string> LogMessages;
        public List<EndPointConfiguration> EndPoints;

        public BaseTest()
        {
        
        }

        [SetUp]
        public void Setup()
        {
            LogMessages = new List<string>();
            string dir = Path.Combine(AppContext.BaseDirectory, "EndPoints.json");
            builder = new ContainerBuilder();
            JObject obj = JsonConvert.DeserializeObject(File.ReadAllText(dir)) as JObject;
            EndPoints = obj["EndPointConfigurations"].ToObject<List<EndPointConfiguration>>();
            registrationHelper = new AutofacRegistrationHelper(builder);
            registrationHelper.RegisterModule(new AdaptiveClientModule(EndPoints, msg => this.LogMessages.Add(msg)));
        }
    }

    public class AdaptiveClientModule : IAdaptiveClientModule
    {
        private List<EndPointConfiguration> EndPoints;
        private Action<string> Logger;

        public AdaptiveClientModule(List<EndPointConfiguration> endPoints, Action<string> logger)
        {
            EndPoints = endPoints;
            Logger = logger;
        }

        public void Register(IRegistrationHelper registrationHelper)
        {
            registrationHelper
                .RegisterEndPoints(EndPoints)
                .Register<InProcessClient1, IDummyAPI1>(EndPointType.InProcess, APINames.DummyAPI1, ProviderName.MSSQL)
                .Register<InProcessClient1, IDummyAPI1>(EndPointType.InProcess, APINames.DummyAPI1)
                .Register<InProcessClient3, IDummyAPI1>(EndPointType.InProcess, APINames.DummyAPI1, ProviderName.MySQL)
                .Register<WebAPIClient1, IDummyAPI1>(EndPointType.HTTP, APINames.DummyAPI1)
                .Register<InProcessClient2, IDummyAPI2>(EndPointType.InProcess, APINames.DummyAPI2)
                .Register<WebAPIClient2, IDummyAPI2>(EndPointType.HTTP, APINames.DummyAPI1)
                .RegisterEndPointValidator<InProcessEndPointValidator>(EndPointType.InProcess, ProviderName.MSSQL)
                .RegisterEndPointValidator<InProcessEndPointValidator>(EndPointType.InProcess, ProviderName.MySQL)
                .RegisterEndPointValidator<InProcessEndPointValidator>(EndPointType.InProcess)
                .RegisterEndPointValidator<HttpEndPointValidator>(EndPointType.WCF, null)
                .RegisterEndPointValidator<HttpEndPointValidator>(EndPointType.HTTP, null)
                .RegisterLogger(Logger);
        }
    }
}
