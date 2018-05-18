using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LeaderAnalytics.AdaptiveClient.Tests
{
    public class HttpEndPointValidator : IEndPointValidator
    {
        private INetworkUtilities networkUtilities;

        public HttpEndPointValidator(INetworkUtilities networkUtilities)
        {
            this.networkUtilities = networkUtilities;
        }

        public bool IsInterfaceAlive(IEndPointConfiguration endPoint)
        {
            return networkUtilities.VerifyHttpServerAvailability(endPoint.ConnectionString);
        }
    }
}
