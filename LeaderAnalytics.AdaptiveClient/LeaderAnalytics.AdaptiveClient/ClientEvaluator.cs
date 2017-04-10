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
                    // All errors are assumed to be connectivity errors. The server is responsible for its own error handling.
                    string errorMsg = $"An error occurred when attempting to connect to EndPointConfiguration named {CachedEndPoint.Name}.  ";
                    errorMsg += $"The service name is {typeof(T).Name}.  The content of the exception is: " + ex.ToString();

                    if (logger != null)
                        logger(errorMsg);
                }
            }

            if (!success)
            {
                Hurl($"A functional EndPointConfiguration could not be resolved for client of type {typeof(T).Name}.", null);
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
                    // All errors are assumed to be connectivity errors. The server is responsible for its own error handling.
                    string errorMsg = $"An error occurred when attempting to connect to EndPointConfiguration named {CachedEndPoint.Name}.  ";
                    errorMsg += $"The service name is {typeof(T).Name}.  The content of the exception is: " + ex.ToString();

                    if (logger != null)
                        logger(errorMsg);
                }
            }

            if (!success)
            {
                Hurl($"A functional EndPointConfiguration could not be resolved for client of type {typeof(T).Name}.", null);
            }
        }
    }
}
