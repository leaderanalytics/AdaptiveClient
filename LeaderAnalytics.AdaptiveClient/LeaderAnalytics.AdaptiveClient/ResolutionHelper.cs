using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;


namespace LeaderAnalytics.AdaptiveClient
{
    public class ResolutionHelper
    {
        public readonly IComponentContext cxt;

        public ResolutionHelper(IComponentContext cxt)
        {
            this.cxt = cxt;
        }
    }
}
