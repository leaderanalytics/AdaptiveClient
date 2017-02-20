using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient
{
    public interface IPerimeter
    {
        string API_Name { get; }
        IList<IEndPointConfiguration> EndPoints { get; }
    }
}
