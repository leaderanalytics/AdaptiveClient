# AdaptiveClient
#### Library and pattern for consuming services across heterogeneous platforms and protocols.  Inject a single client that allows the application to transparently access API's using SQL client, WebAPI, REST, WCF, ESB, etc.  Gracefully fall back if preferred server or protocol becomes unavailable.

```C#
public partial class MainWindow : Window
{
    private IAdaptiveClient<IUsersService> client;

    public MainWindow(IAdaptiveClient<IUsersService> client)
    {
        this.client = client;
    }

    public async Task<IActionResult> Login(int userID)
    {
        // AdaptiveClient will use the best server available at the time the request is made. 
        // Server may be SQL, WCF, REST, etc. - your application does not need to know or care.
        // If the request fails AdaptiveClient will begin an orderly fall back to other 
        // servers that can handle the request regardless of platform or protocol:

        User user = await client.CallAsync(x => x.GetUser(userID));
    }
}
```

&nbsp;

---
#### [Get the simple console app demo](https://github.com/leaderanalytics/AdaptiveClient.SimpleConsoleDemo)

---
#### [Get the advanced end-to-end demo](https://github.com/leaderanalytics/AdaptiveClient.AdvancedDemo)

---

#### [Get the nuget package](https://www.nuget.org/packages/AdaptiveClient/)

---

&nbsp;



## What AdaptiveClient does
Rather than make a service call directly to a specific server or type of server you make a call using `AdaptiveClient` instead.  `AdaptiveClient` will attempt to execute the call using the best available server.  If the call fails `AdaptiveClient` will make successive attempts, each time falling back to other servers of the same type or other types.

For example, a mobile user who is on-site and connected to a local area network will enjoy the performance of an in-process connection directly to the database server.  If the user tries to re-connect from a remote location `AdaptiveClient` will attempt a LAN connection again but will fall back to a WebAPI server when the LAN connection fails.  Should the WebAPI connection fail, `AdaptiveClient` may attempt to connect to other WebAPI servers, a WCF server, or any other server as configured.

## Who will benefit from using it
* `AdaptiveClient` is ideally targeted to organizations that need to give local users access to their APIs over a local area network but who also wish to expose their APIs to remote users.
* Developers who want to implement retry and/or fall back logic when making service calls.


## How it works
`AdaptiveClient` is a design pattern that leverages [n-tier architecture](https://en.wikipedia.org/wiki/Multitier_architecture) and a dependency injection container.  The client and utility classes included in this download assist you in implementing the pattern.  This download includes a client implementation based on Autofac.  You should be able to implement similar functionality using other DI containers.  

The functionality provided by `AdaptiveClient` comes primarily from the four classes shown below and their supporting classes:


    EndPointConfiguration

An `EndPointConfiguration` (a.k.a EndPoint for short) is like a connection string or a URL but it includes some extra properties that are useful:

* **Name**: Name of the EndPoint: DevServer01, QASloth02, etc.
* **API_Name**:  Name of the application or API exposed by the EndPoint: OurCompanyApp, xyz.com, etc.  NOT the name of a contract or interface.
* **Preference**:  Number that allows ClientFactory to rank this EndPoint.  Lower numbers are ranked higher (more preferred).
* **EndPointType**:  May be one of the following:  InProcess, HTTP, WCF, ESB.  Assists ClientFactory in determining if the EndPoint is alive.  Multiple EndPointConfigurations of the same `EndPointType` may be defined for an API_Name.
* **ConnectionString**:  Valid connection string OR URL if pointing to a HTTP server.
* **Parameters**:  Not used at this time.
* **IsActive**:  Set this value to false to prevent using this `EndPointConfiguration`.

&nbsp;
     
    ClientFactory

Given an interface and a collection of `EndPointConfiguration` objects,  `ClientFactory` will iterate over the EndPoints starting with the most preferred.  Upon finding an available EndPoint `ClientFactory` will return a suitable client that implements the desired interface.


    RegistrationHelper

RegistrationHelper is one of two Autofac-specific classes.  `RegistrationHelper` hides the complexity of registering  `EndPointConfiguration` objects and clients with the DI container.  Usage is discussed in the Getting Started section.  

    AdaptiveClient

`AdaptiveClient`  is the second of the two Autofac-specific classes.  `AdaptiveClient` is little more than a wrapper around ClientFactory that insures that objects created within one of the `AdaptiveClient.Call()` methods are created and disposed within an Autofac LifetimeScope.  If you choose to use the `AdaptiveClient` pattern with a DI container other than Autofac you can use `ClientFactory` as required instead of `AdaptiveClient` and implement scope logic as required by your DI container. 


## How `AdaptiveClient` resolves a client from start to finish: 

![alt an image](https://raw.githubusercontent.com/leaderanalytics/AdaptiveClient/master/LeaderAnalytics.AdaptiveClient/docs/HowAdaptiveClientWorks.png)



## Getting started



1. Define your `EndPointConfiguration` objects.  See appsettings.development.json in WebAPIServer project of the Demo application.


2. Register your `EndPointConfiguration` objects. Use RegistrationHelper as shown in the section below.

3. Register your domain services and clients as shown in the section below.  See also the AdaptiveClientModule file in the Application.Services project of the Demo application.  

4. Accept `IAdaptiveClient<T>` or `IClientFactory<T>` in your constructor wherever you need a client.  `IAdaptiveClient` calls `IClientFactory` internally and it disposes the objects it creates within the Call method.

#### Using `RegistrationHelper`
Follow the two steps below to register your `EndPointConfiguration` objects and clients.

 
1. Register the entire collection of `EndPointConfiguration` objects for an API or Application:

  ```C#
 RegistrationHelper registrationHelper = new RegistrationHelper(builder);
 IEnumerable<IEndPointConfiguration> endPoints = ... // read endpoints from config file 
 registrationHelper.RegisterEndPoints(endPoints);
 ```
  
 * `EndPointConfiguration` objects must be registered **before** clients are registered.
 * `RegistrationHelper` only registers clients and `EndPointConfiguration` objects.  You must register other objects in your application as you normally do using your DI container.
  
2. Register each combination of client and `EndPointType` that is implemented by your application.  Three examples are shown below but only EndPointTypes you actually use are required.      

 ```C#
 string apiName = "OurCompanyAPI";
 // client that communicates directly with the database (the service itself)
 registrationHelper.Register<MyApp.Services.UsersService, IUsersService>(EndPointType.InProcess, apiName);
 // WebAPI client 
 registrationHelper.Register<MyApp.WebAPIClient.UsersClient, IUsersService>(EndPointType.HTTP, apiName);
 // WCF client 
 registrationHelper.Register<MyApp.WCFClient.UsersClient, IUsersService>(EndPointType.WCF, apiName);
 ```
 &nbsp;

## Tips & FAQs

* `AdaptiveClient` is designed to work with an n-tier architecture. Make sure your application has clean separation of layers. Generally this means your business logic should reside entirely in your service layer - not in controllers, code-behind, or view models.

* Create clients in their own assemblies as required.  Clients must implement the same interfaces as their server counterparts and the services they access.  Register clients the same way services are registered.  See the Application.WebAPIClient project for an example.

* Will `AdaptiveClient` make multiple hops to resolve a client? Yes, see the demo at the link below.

* Can I force `AdaptiveClient` to use a certain EndPoint and bypass the fallback logic? Yes. You can set the IsActive flag to false for EndPointConfigurations you dont want to use.  You can also supply one or more EndPoint names in your call to `AdaptiveClient` or ClientFactory:

```C#
User user = await client.CallAsync(x => x.GetUser(userID), "MyEndPointName");
```

---
#### [Get the simple console app demo](https://github.com/leaderanalytics/AdaptiveClient.SimpleConsoleDemo)

---
#### [Get the advanced end-to-end demo](https://github.com/leaderanalytics/AdaptiveClient.AdvancedDemo)

---

