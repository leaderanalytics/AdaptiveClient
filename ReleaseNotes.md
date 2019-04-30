# AdaptiveClient Release Notes
## Version 0.32.0 (2019-04-30)
Services no longer need to implement `IDisposable`.

## Version 0.31.3 (2018-11-21)
Save validation result so CachedEndpoint is validated once only.

## Version 0.30.00 (2018-05-12)
### Breaking changes

1. `RegistrationHelper.Register` was renamed `RegistrationHelper.RegisterService`.  An overload was added that takes an `IEndPointConfiguration`.
2. ProviderName is now required when registering a service or `IEndPointValidator`. 
3. Valid ProviderName is required for all `EndPointConfigurations`.
4. `IRegistrationHelper` interface has been deprecated.

### Other changes
* Added `EndPointUtilities` class.
* Fixed a bug in `AdaptiveClient.Try`.
* Refactored Registration and Resolution methods into extension methods.


## Version 0.16.7 (2017-03-15)
### Breaking changes

1. The `EndPointType` enum was replaced with a string.  The purpose is to allow greater flexibility when matching a client implementation to a specific type or version of server.  Suggested replacement for `EndPointType` enum is a simple class like the following:

```
public static class EndPointType
{
    public const string SQLServer = "SQLServer"; // InProcess
    public const string Oracle = "Oracle";       // Also InProcess
    public const string HTTP = "HTTP";
    public const string WCF = "WCF";
    public const string ESB = "ESB";
    public const string File = "File";
    public const string FTP = "FTP";
}
```

2. Removed RegistrationHelper methods for WCF clients.  Those registrations belong in the application.

### Improvements
Cleaned up unnecessary dependencies.

---
#### [AdaptiveClient](https://github.com/leaderanalytics/AdaptiveClient)

---

