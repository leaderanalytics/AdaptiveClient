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
            Func<EndPointType, T> serviceFactory,
            EndPointContext endPointContext) : base(epcFactory, serviceFactory, null, endPointContext)
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
                    if (!IsConnectivityException(ex))
                    {
                        string errorMsg = $"An error occurred that appears to be unrelated to client connectivity.  This is most likely an application error. ";
                        errorMsg += $"The service name is {typeof(T).Name}.  The EndPointConfiguration Name is {CurrentEndPoint.Name}.  See InnerException for details";
                        Hurl(errorMsg, ex);
                    }
                }
            }

            if (!success)
            {
                Hurl($"A functional EndPointConfiguration could not be resolved for client of type {typeof(T).Name}.", null);
            }
        }

        protected virtual bool IsConnectivityException(Exception ex)
        {
            return true;
        }
    }
}
