using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace LeaderAnalytics.AdaptiveClient
{
    public class RegistrationHelper
    {
        public readonly ContainerBuilder Builder;
        public Dictionary<string, IPerimeter> EndPointDict;
        public Dictionary<string, string> ServiceRegistrations;

        public RegistrationHelper(ContainerBuilder builder)
        {
            this.Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            builder.RegisterModule(new AutofacModule());
            EndPointDict = new Dictionary<string, IPerimeter>();
            ServiceRegistrations = new Dictionary<string, string>();
        }
    }
}
