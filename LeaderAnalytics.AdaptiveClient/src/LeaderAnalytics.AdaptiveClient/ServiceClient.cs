using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;


namespace LeaderAnalytics.AdaptiveClient
{

    // Todo:  Some redundant code here - need to refactor.  We don't want the consumer of this class to have to 
    // deal with disposing of it. 

    public class ServiceClient<T> : IServiceClient<T> where T : class, IDisposable
    {
        public IEndPointConfiguration CurrentEndPoint { get; private set; }
        private ILifetimeScope container;

        public ServiceClient(ILifetimeScope container)
        {
            this.container = container;
        }

        public void Call(Action<T> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientResolver<T> resolver = scope.Resolve<IClientResolver<T>>();
                T client = resolver.ResolveClient(endPointNames);
                CurrentEndPoint = resolver.CurrentEndPoint;
                method(client);
            }
        }

        public TResult Call<TResult>(Func<T, TResult> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientResolver<T> resolver = scope.Resolve<IClientResolver<T>>();
                T client = resolver.ResolveClient(endPointNames);
                CurrentEndPoint = resolver.CurrentEndPoint;
                return method(client);
            }
        }

        public async Task CallAsync(Func<T, Task> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientResolver<T> resolver = scope.Resolve<IClientResolver<T>>();
                T client = resolver.ResolveClient(endPointNames);
                CurrentEndPoint = resolver.CurrentEndPoint;
                await method(client);
            }
        }

        public async Task<TResult> CallAsync<TResult>(Func<T, Task<TResult>> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientResolver<T> resolver = scope.Resolve<IClientResolver<T>>();
                T client = resolver.ResolveClient(endPointNames);
                CurrentEndPoint = resolver.CurrentEndPoint;
                return await method(client);
            }
        }
    }
}
