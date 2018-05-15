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
            EndPoints = EndPointUtilities.LoadEndPoints("EndPoints.json").ToList();
            registrationHelper = new RegistrationHelper(builder);
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
                
                .RegisterEndPointValidator<InProcessEndPointValidator>(EndPointType.InProcess, ProviderName.MSSQL)
                .RegisterEndPointValidator<InProcessEndPointValidator>(EndPointType.InProcess, ProviderName.MySQL)
                .RegisterEndPointValidator<HttpEndPointValidator>(EndPointType.WCF, ProviderName.HTTP)
                .RegisterEndPointValidator<HttpEndPointValidator>(EndPointType.HTTP, ProviderName.HTTP)
                .RegisterLogger(Logger);
        }
    }
}
