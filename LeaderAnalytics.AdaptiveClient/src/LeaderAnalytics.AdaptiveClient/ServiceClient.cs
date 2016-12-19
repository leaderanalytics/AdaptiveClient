using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;


namespace LeaderAnalytics.AdaptiveClient
{
    public class ServiceClient<T> : IServiceClient<T> where T : class, IDisposable
    {
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
                method(client);
            }
        }

        public TResult Call<TResult>(Func<T, TResult> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientResolver<T> resolver = scope.Resolve<IClientResolver<T>>();
                T client = resolver.ResolveClient(endPointNames);
                return method(client);
            }
        }

        public async Task CallAsync(Func<T, Task> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientResolver<T> resolver = scope.Resolve<IClientResolver<T>>();
                T client = resolver.ResolveClient(endPointNames);
                await method(client);
            }
        }

        public async Task<TResult> CallAsync<TResult>(Func<T, Task<TResult>> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientResolver<T> resolver = scope.Resolve<IClientResolver<T>>();
                T client = resolver.ResolveClient(endPointNames);
                return await method(client);
            }
        }
    }
}
