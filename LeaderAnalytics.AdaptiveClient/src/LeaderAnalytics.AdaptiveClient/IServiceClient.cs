using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient
{
    public interface IServiceClient<T> where T : class, IDisposable
    {
        void Try(Action<T> method, params string[] endPointNames);
        TResult Try<TResult>(Func<T, TResult> method, params string[] endPointNames);
        Task TryAsync(Func<T, Task> method, params string[] endPointNames);
        Task<TResult> TryAsync<TResult>(Func<T, Task<TResult>> method, params string[] endPointNames);
    }
}
