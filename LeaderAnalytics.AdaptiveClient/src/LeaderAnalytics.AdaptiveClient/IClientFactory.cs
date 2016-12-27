using System;
using System.Collections.Generic;

namespace LeaderAnalytics.AdaptiveClient
{
    public interface IClientFactory<T> 
    {
        IEndPointConfiguration CurrentEndPoint { get; }
        T Create(params string[] overrideNames);
    }
}