using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace LeaderAnalytics.AdaptiveClient.Autofac
{

   
    public class AdaptiveClient<T> : IAdaptiveClient<T> where T : class
    {
        private EndPointCache endPointContext;
        private IPerimeter perimeter;
        private ILifetimeScope container;
        public IEndPointConfiguration CurrentEndPoint => endPointContext.GetEndPoint(perimeter.API_Name);

        public AdaptiveClient(ILifetimeScope container, Func<Type, IPerimeter> epcFactory, EndPointCache endPointCache)
        {
            this.container = container;
            perimeter = epcFactory(typeof(T));
            this.endPointContext = endPointCache;
        }

        public void Call(Action<T> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientFactory<T> factory = scope.Resolve<IClientFactory<T>>();
                T client = factory.Create(endPointNames);
                method(client);
            }
            
        }

        public TResult Call<TResult>(Func<T, TResult> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientFactory<T> factory = scope.Resolve<IClientFactory<T>>();
                T client = factory.Create(endPointNames);
                return method(client);
            }
        }

        public async Task CallAsync(Func<T, Task> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientFactory<T> factory = scope.Resolve<IClientFactory<T>>();
                T client = factory.Create(endPointNames);
                await method(client);
            }
        }

        public async Task<TResult> CallAsync<TResult>(Func<T, Task<TResult>> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientFactory<T> factory = scope.Resolve<IClientFactory<T>>();
                T client = factory.Create(endPointNames);
                return await method(client);
            }
        }

        public void Try(Action<T> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientEvaluator<T> evaluator = scope.Resolve<IClientEvaluator<T>>();
                evaluator.Try(method, endPointNames);
            }
        }

        public TResult Try<TResult>(Func<T, TResult> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientEvaluator<T> evaluator = scope.Resolve<IClientEvaluator<T>>();
                TResult result = evaluator.Try(method, endPointNames);
                return result;
            }
        }

        public async Task TryAsync(Func<T, Task> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientEvaluator<T> evaluator = scope.Resolve<IClientEvaluator<T>>();
                await evaluator.TryAsync(method, endPointNames);
            }
        }

        public async Task<TResult> TryAsync<TResult>(Func<T, Task<TResult>> method, params string[] endPointNames)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                IClientEvaluator<T> evaluator = scope.Resolve<IClientEvaluator<T>>();
                TResult result = await evaluator.TryAsync(method, endPointNames);
                return result;
            }
        }
    }
}
