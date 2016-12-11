using System;
using System.Collections.Generic;
using Autofac;


namespace LeaderAnalytics.AdaptiveClient
{
    public interface IClientResolver<T> 
    {
        T ResolveClient(params string[] overrideNames);
    }
}