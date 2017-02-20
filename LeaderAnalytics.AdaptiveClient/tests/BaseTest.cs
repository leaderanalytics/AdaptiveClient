using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Autofac;
using LeaderAnalytics.AdaptiveClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeaderAnalytics.AdaptiveClient.Tests
{
    public class BaseTest
    {
        public ContainerBuilder builder;
        public AutofacRegistrationHelper registrationHelper;
        public string LogMessage;

        [TestInitialize]
        public void Setup()
        {
            string dir = Path.Combine(Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.FullName,"EndPoints.json");
            
            builder = new ContainerBuilder();
            JObject obj = JsonConvert.DeserializeObject(File.ReadAllText(dir)) as JObject;
            List<EndPointConfiguration> endPoints = obj["EndPointConfigurations"].ToObject<List<EndPointConfiguration>>();

            registrationHelper = new AutofacRegistrationHelper(builder);
            registrationHelper.RegisterEndPoints(endPoints);

            registrationHelper.Register<InProcessClient1, IDummyAPI1>(EndPointType.InProcess, APINames.DummyAPI1.ToString());
            registrationHelper.Register<WebAPIClient1, IDummyAPI1>(EndPointType.WebAPI, APINames.DummyAPI1.ToString());

            registrationHelper.Register<InProcessClient2, IDummyAPI2>(EndPointType.InProcess, APINames.DummyAPI2.ToString());
            registrationHelper.Register<WebAPIClient2, IDummyAPI2>(EndPointType.WebAPI, APINames.DummyAPI1.ToString());
            registrationHelper.RegisterLogger(msg => this.LogMessage = msg);
        }
    }
}
