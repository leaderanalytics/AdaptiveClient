using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient
{
    public class ClientEvaluator<T> : BaseClientFactory<T>, IClientEvaluator<T>
    {
        public ClientEvaluator(
            Func<Type, IPerimeter> epcFactory,
            Func<string, T> serviceFactory,
            EndPointCache endPointCache,
            EndPointContext endPointContext,
            Action<string> logger) : base(epcFactory, serviceFactory, null, endPointCache, endPointContext, logger)
        {
          
        }

        public void Try(Action<T> method, params string[] overrideNames)
        {
            TryInternal(method, overrideNames);
        }

        public TResult Try<TResult>(Func<T, TResult> method, params string[] overrideNames)
        {
            TResult result = default(TResult);
            Action<T> proxy = x => result = method(x);
            TryInternal(proxy, overrideNames);
            return result;
        }

        private void TryInternal(Action<T> method, params string[] overrideNames)
        {
            SetAvailableEndPoints(overrideNames);
            bool success = false;
            List<Exception> exceptions = null;

            foreach (T client in ClientEnumerator())
            {
                try
                {
                    method(client);
                    success = true;
                    break;
                }
                catch (Exception ex)
                {
                    string errorMsg = $"An error occurred when attempting to connect to EndPointConfiguration named {CachedEndPoint.Name}. The service name is {typeof(T).Name}. The connection string is {CachedEndPoint.ConnectionString}. See the inner exception for more detail.";

                    if (exceptions == null)
                        exceptions = new List<Exception>(10);

                    exceptions.Add(new Exception(errorMsg, ex));  // We log but don't throw unless we run out of endpoints

                    if (logger != null)
                        logger(errorMsg);
                }
            }

            if (!success)
            {
                AggregateException aggEx = null;

                if (exceptions != null)
                    aggEx = new AggregateException(exceptions);

                Hurl($"A functional EndPointConfiguration could not be resolved for client of type {typeof(T).Name}.  See inner exception(s) for more detail.", aggEx);
            }
        }

        public async Task<TResult> TryAsync<TResult>(Func<T, Task<TResult>> method, params string[] overrideNames)
        {
            TResult result = default(TResult);
            Func<T, Task> proxy = async x => result = await method(x);
            await TryInternalAsync(proxy, overrideNames);
            return result;
        }

        public async Task TryAsync(Func<T, Task> method, params string[] overrideNames)
        {
            await TryInternalAsync(method, overrideNames);
        }

        private async Task TryInternalAsync(Func<T, Task> method, params string[] overrideNames)
        {
            SetAvailableEndPoints(overrideNames);
            bool success = false;
            List<Exception> exceptions = null;

            foreach (T client in ClientEnumerator())
            {
                try
                {
                    await method(client);
                    success = true;
                    break;
                }
                catch (Exception ex)
                {
                    string errorMsg = $"An error occurred when attempting to connect to EndPointConfiguration named {CachedEndPoint.Name}. The service name is {typeof(T).Name}. The connection string is {CachedEndPoint.ConnectionString}. See the inner exception for more detail.";

                    if (exceptions == null)
                        exceptions = new List<Exception>(10);

                    exceptions.Add(new Exception(errorMsg, ex));  // We log but don't throw unless we run out of endpoints

                    if (logger != null)
                        logger(errorMsg);
                }
            }

            if (!success)
            {
                AggregateException aggEx = null;

                if (exceptions != null)
                    aggEx = new AggregateException(exceptions);

                Hurl($"A functional EndPointConfiguration could not be resolved for client of type {typeof(T).Name}.  See inner exception(s) for more detail.", aggEx);
            }
        }
    }
}
