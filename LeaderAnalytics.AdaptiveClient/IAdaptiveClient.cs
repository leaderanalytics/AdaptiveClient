namespace LeaderAnalytics.AdaptiveClient;

public interface IAdaptiveClient<T> where T : class
{
    /// <summary>
    /// Returns the last resolved EndPoint, if any.
    /// </summary>
    IEndPointConfiguration CurrentEndPoint { get; }

    /// <summary>
    /// Calls method using CurrentEndPoint. If no CurrentEndPoint, an EndPoint is resolved using an EndPointValidator.
    /// Once a valid EndPoint is identified, method is called.  There is no fallback if the call fails.
    /// </summary>
    /// <param name="method">Method to call.</param>
    /// <param name="endPointNames">Optional list of EndPoints to use instead of registered EndPoints.</param>
    void Call(Action<T> method, params string[] endPointNames);

    /// <summary>
    /// Calls method returning TResult using CurrentEndPoint. If no CurrentEndPoint, an EndPoint is resolved using an EndPointValidator.
    /// Once a valid EndPoint is identified, method is called.  There is no fallback if the call fails.
    /// </summary>
    /// <param name="method">Method to call.</param>
    /// <param name="endPointNames">Optional list of EndPoints to use instead of registered EndPoints.</param>
    TResult Call<TResult>(Func<T, TResult> method, params string[] endPointNames);

    /// <summary>
    /// Asynchronously calls method using CurrentEndPoint. If no CurrentEndPoint, an EndPoint is resolved using an EndPointValidator.
    /// Once a valid EndPoint is identified, method is called.  There is no fallback if the call fails.
    /// </summary>
    /// <param name="method">Method to call.</param>
    /// <param name="endPointNames">Optional list of EndPoints to use instead of registered EndPoints.</param>
    Task CallAsync(Func<T, Task> method, params string[] endPointNames);

    /// <summary>
    /// Asynchronously calls method returning TResult using CurrentEndPoint. If no CurrentEndPoint, an EndPoint is resolved using an EndPointValidator.
    /// Once a valid EndPoint is identified, method is called.  There is no fallback if the call fails.
    /// </summary>
    /// <param name="method">Method to call.</param>
    /// <param name="endPointNames">Optional list of EndPoints to use instead of registered EndPoints.</param>
    Task<TResult> CallAsync<TResult>(Func<T, Task<TResult>> method, params string[] endPointNames);

    /// <summary>
    /// Calls method in a try block using CurrentEndPoint or first registered EndPoint.  If the call fails the next
    /// registered EndPoint is used, and so on.  
    /// </summary>
    /// <param name="method">Method to call.</param>
    /// <param name="endPointNames">Optional list of EndPoints to use instead of registered EndPoints.</param>
    void Try(Action<T> evaluator, params string[] endPointNames);

    /// <summary>
    /// Calls method returning TResult in a try block using CurrentEndPoint or first registered EndPoint.  If the call fails the next
    /// registered EndPoint is used, and so on.  
    /// </summary>
    /// <param name="method">Method to call.</param>
    /// <param name="endPointNames">Optional list of EndPoints to use instead of registered EndPoints.</param>
    TResult Try<TResult>(Func<T, TResult> method, params string[] endPointNames);

    /// <summary>
    /// Asynchronously calls method in a try block using CurrentEndPoint or first registered EndPoint.  If the call fails the next
    /// registered EndPoint is used, and so on.  
    /// </summary>
    /// <param name="method">Method to call.</param>
    /// <param name="endPointNames">Optional list of EndPoints to use instead of registered EndPoints.</param>
    Task TryAsync(Func<T, Task> method, params string[] overrideNames);

    /// <summary>
    /// Asynchronously calls method returning TResult in a try block using CurrentEndPoint or first registered EndPoint.  If the call fails the next
    /// registered EndPoint is used, and so on.  
    /// </summary>
    /// <param name="method">Method to call.</param>
    /// <param name="endPointNames">Optional list of EndPoints to use instead of registered EndPoints.</param>
    Task<TResult> TryAsync<TResult>(Func<T, Task<TResult>> method, params string[] endPointNames);
}
