# AdaptiveClient
Pattern and utility classes to resolve and use any available database, WebAPI, WCF, or ESB endpoint on the fly at run time with a single call.
```C#
public class HomeController : Controller
{
    private IAdaptiveClient<IUsersService> client;

    public HomeController(IAdaptiveClient<IUsersService> client)
    {
        this.client = client;
    }

    public async Task<IActionResult> Login(int userID)
    {
        // use an in-process connection to the database if its available otherwise use
        // whatever server is available to handle the request (WebAPI, WCF, etc.):
        User user = await client.CallAsync(x => x.GetUser(userID));
    }
}
```

&nbsp;

---
#### [Check out the end-to-end demo here](https://github.com/leaderanalytics/AdaptiveClientDemo)

---

&nbsp;



## What it does
Rather than make calls directly to a specific type of server you make service calls using `AdaptiveClient` instead.  `AdaptiveClient` will make successive attempts to use the most preferred transport while falling back if a transport or server is unavailable.  For example, a mobile user who is on-site and connected to a local area network will enjoy the performance benefit of an in-process connection directly to the database server.  When the user is off-site AdaptiveClient will attempt a LAN connection again but will fall back to a WebAPI server when the LAN connection fails.  Should the WebAPI connection fail, AdaptiveClient may attempt to connect to other WebAPI servers, a WCF server, or any other server as configured.

## Who will benefit from using it
* AdaptiveClient is ideally targeted to organizations that need to consume their API using direct database connections over a LAN but who also wish to expose their API using a protocol such as HTTP or SOAP.
* Applications that consume an API exposed via multiple servers using dissimilar protocols will benefit.
* Applications that need retry and/or fall back logic when making service calls will also benefit.


## How it works
AdaptiveClient is a design pattern that leverages [n-tier architecture](https://en.wikipedia.org/wiki/Multitier_architecture) and a dependency injection container.  The client and utility classes included in this download assist you in implementing the pattern.  This download includes a client implementation based on Autofac.  You should be able to implement similar functionality using other DI containers.  

The functionality provided by AdaptiveClient comes primarily from the four classes shown below and their supporting classes:


    EndPointConfiguration

An `EndPointConfiguration` is like a connection string or a URL but it includes some extra properties that are useful:

* **Name**: Name of the EndPoint: DevServer01, QASloth02, etc.
* **API_Name**:  Name of the application or API exposed by the EndPoint: OurCompanyApp, xyz.com, etc.  NOT the name of a contract or interface.
* **Preference**:  Number that allows ClientFactory to rank this EndPoint.  Lower numbers are ranked higher (more preferred).
* **EndPointType**:  May be one of the following:  InProcess, WebAPI, WCF, ESB.  Assists ClientFactory in determining if the EndPoint is alive.  Multiple EndPointConfigurations of the same `EndPointType` may be defined for an API_Name.
* **ConnectionString**:  Valid connection string OR URL if pointing to a HTTP server.
* **Parameters**:  Not used at this time.
* **IsActive**:  Set this value to false to prevent using this `EndPointConfiguration`.

&nbsp;
     
    ClientFactory

Given an interface and a collection of `EndPointConfiguration` objects,  `ClientFactory` will iterate over the EndPoints starting with the most preferred.  Upon finding an available EndPoint `ClientFactory` will return a suitable client that implements the desired interface.


    RegistrationHelper

RegistrationHelper is one of two Autofac-specific classes.  `RegistrationHelper` hides the complexity of registering  `EndPointConfiguration` objects and clients with the DI container.  Usage is discussed in the Getting Started section.  

    AdaptiveClient

`AdaptiveClient`  is the second of the two Autofac-specific classes.  AdaptiveClient is little more than a wrapper around ClientFactory that insures that objects created within one of the `AdaptiveClient.Call()` methods are created and disposed within an Autofac LifetimeScope.  If you choose to use the AdaptiveClient pattern with a DI container other than Autofac you can use `ClientFactory` as required instead of `AdaptiveClient` and implement scope logic as required by your DI container. 


![alt an image](https://raw.githubusercontent.com/leaderanalytics/AdaptiveClient/master/LeaderAnalytics.AdaptiveClient/docs/HowAdaptiveClientWorks.png)



## Getting started



1. AdaptiveClient is designed to work with an n-tier architecture.  Make sure your application has clean separation of layers.  Generally this means your business logic should reside entirely in your service layer.
2. Define your EndPointConfigurations.  See appsettings.development.json in WebAPIServer project of the Demo application.
3. Its a good idea to put your domain services, domain models, and domain interfaces in their own individual assemblies. 
5. Register your domain services as shown in the `AdaptiveClientModule` file in the Application.Services project of the Demo application.  
6. Create clients in their own assemblies as required.  Clients must implement the same interfaces as their server counterparts and the services they access.  Register clients the same way services are registered.  See the Application.WebAPIClient project for an example.
7. Create API servers as required.  See the WCFServer and the WebAPIServer projects in the Demo application.  Note that controllers in the WebAPIServer example implement domain interfaces even though it is not technically required to do so.
8. Create your presentation layer.  Applications can be Web, WPF, Xamarin, Winforms, etc.  See the WebApplication and WPFApplication examples in the Demo.  

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
  
2. Register each combination of client and `EndPointType` that is implemented by your application.    Three examples are shown below but only EndPointTypes you actually use are required.      

 ```C#
 string apiName = "OurCompanyAPI";
 // client that communicates directly with the database (the service itself)
 registrationHelper.Register<MyApp.Services.UsersService, IUsersService>(EndPointType.InProcess, apiName);
 // WebAPI client 
 registrationHelper.Register<MyApp.WebAPIClient.UsersClient, IUsersService>(EndPointType.WebAPI, apiName);
 // WCF client 
 registrationHelper.Register<MyApp.WCFClient.UsersClient, IUsersService>(EndPointType.WCF, apiName);
 ```
 &nbsp;

 
---
#### [Check out the end-to-end demo here](https://github.com/leaderanalytics/AdaptiveClientDemo)

---

