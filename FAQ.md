# AdaptiveClient FAQ

## How to inject the current EndPoint:

```
public void SomeMethod(Func<IEndPointConfiguration> endPointFactory)
{
    IEndPointConfiguration endPoint = endPointFactory();
    // use endPoint
}
```


## Differences between `ClientFactory` and `ClientEvaluator`

`ClientFactory`

* Uses an `EndPointValidator` to determine if an EndPoint is available.  This requires one or more additional network calls.
* Returns an instance of a client
* Subsequent calls to the EndPoint using the returned client offer no assurance the EndPoint is still available

`ClientEvaluator`
* Executes your method in a `try` block.  If an Exception is thrown the process is repeated on remaining EndPoints.
* If the call is successful the result is returned, if any.   

---
#### [AdaptiveClient](https://github.com/leaderanalytics/AdaptiveClient)

---

