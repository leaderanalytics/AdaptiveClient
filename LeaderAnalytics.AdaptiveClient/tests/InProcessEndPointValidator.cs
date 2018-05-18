using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LeaderAnalytics.AdaptiveClient.Tests
{
    public class InProcessEndPointValidator : IEndPointValidator
    {
        private INetworkUtilities networkUtilities;

        public InProcessEndPointValidator(INetworkUtilities networkUtilities)
        {
            this.networkUtilities = networkUtilities;
        }

        public virtual bool IsInterfaceAlive(IEndPointConfiguration endPoint)
        {
            return networkUtilities.VerifyDBServerConnectivity(endPoint.ConnectionString);
        }
    }
}
