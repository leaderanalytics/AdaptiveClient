using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient
{
    public interface IClientEvaluator<T>
    {
        void Try(Action<T> evaluator, params string[] overrideNames);
        TResult Try<TResult>(Func<T, TResult> evaluator, params string[] endPointNames);
    }
}
