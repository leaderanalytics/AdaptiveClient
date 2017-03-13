# AdaptiveClient Release Notes

## Version 0.16.5 (2017-03-01)
### Breaking change
The `EndPointType` enum was replaced with a string.  The purpose is to allow greater flexibility when matching a client implementation of a specific version to a server.  Suggested replacement for `EndPointType` enum is a simple class like the following:

```
public static class EndPointType
{
    public const string InProcess = "InProcess";
    public const string HTTP = "HTTP";
    public const string WCF = "WCF";
    public const string ESB = "ESB";
    public const string File = "File";
    public const string FTP = "FTP";
}
```

Removed RegistrationHelper methods for WCF clients.  Those registrations belong in the application.

### Improvements
Cleaned up unnecessary dependencies.

---
#### [AdaptiveClient](https://github.com/leaderanalytics/AdaptiveClient)

---

