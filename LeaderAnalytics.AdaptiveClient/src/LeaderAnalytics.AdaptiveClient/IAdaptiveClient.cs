using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient
{
    public interface IAdaptiveClient<T> where T : class, IDisposable
    {
        IEndPointConfiguration CurrentEndPoint { get; }
        void Call(Action<T> method, params string[] endPointNames);
        TResult Call<TResult>(Func<T, TResult> method, params string[] endPointNames);
        Task CallAsync(Func<T, Task> method, params string[] endPointNames);
        Task<TResult> CallAsync<TResult>(Func<T, Task<TResult>> method, params string[] endPointNames);
    }
}
