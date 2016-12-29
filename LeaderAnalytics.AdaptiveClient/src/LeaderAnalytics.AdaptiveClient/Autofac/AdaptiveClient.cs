using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace LeaderAnalytics.AdaptiveClient.Autofac
{

    // Todo:  Some redundant code here - need to refactor.  Dont want to implement IDisposable we don't want the consumer of this class to have to 
    // deal with disposing of it. 

    public class AdaptiveClient<T> : IAdaptiveClient<T> where T : class, IDisposable
    {
        public IEndPointConfiguration CurrentEndPoint { get; private set; }
        private ILifetimeScope container;

        public AdaptiveClient(ILifetimeScope container)
        {
            this.container = container;
        }

        public void Call(Action<T> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientFactory<T> factory = scope.Resolve<IClientFactory<T>>();
                T client = factory.Create(endPointNames);
                CurrentEndPoint = factory.CurrentEndPoint;
                method(client);
            }
        }

        public TResult Call<TResult>(Func<T, TResult> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientFactory<T> factory = scope.Resolve<IClientFactory<T>>();
                T client = factory.Create(endPointNames);
                CurrentEndPoint = factory.CurrentEndPoint;
                return method(client);
            }
        }

        public async Task CallAsync(Func<T, Task> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientFactory<T> factory = scope.Resolve<IClientFactory<T>>();
                T client = factory.Create(endPointNames);
                CurrentEndPoint = factory.CurrentEndPoint;
                await method(client);
            }
        }

        public async Task<TResult> CallAsync<TResult>(Func<T, Task<TResult>> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientFactory<T> factory = scope.Resolve<IClientFactory<T>>();
                T client = factory.Create(endPointNames);
                CurrentEndPoint = factory.CurrentEndPoint;
                return await method(client);
            }
        }
    }
}
