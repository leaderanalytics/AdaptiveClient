using System;
using System.Collections.Generic;
using Autofac;


namespace LeaderAnalytics.AdaptiveClient
{
    public interface IClientResolver<T> 
    {
        IEndPointConfiguration CurrentEndPoint { get; }
        T ResolveClient(params string[] overrideNames);
    }
}