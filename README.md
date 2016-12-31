# AdaptiveClient
A pattern and lightweight client to resolve and use any available database, REST, WCF, or ESB endpoint on the fly at run time with a single call.
```csharp
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
        // a REST server:
        User user = await client.CallAsync(x => x.GetUser(userID));
    }
}
```


## What it does
Rather than make calls directly to a specific type of server you make service calls using AdaptiveClient instead.  The client will make repeated attempts to use the fastest available transport while falling back if a transport or server is unavilable.  For example, a mobile user who is connected to a local area network will enjoy the performance benefit of an in-process connection directly to the database server.  Later when the user is off-site they will connect via a public REST server with no configuration changes.

## Who will benefit from using it
* AdaptiveClient is ideally targeted to organizations that need to consume their API using direct database connections over a LAN but who also wish to expose their API using a protocol such as HTTP or SOAP.
* Applications that consume an API exposed via multiple servers using dissimilar protocols will benefit.
* Applications that need retry and/or fallback logic when making service callls will also benefit.


## How it works
AdaptiveClient is a design pattern that leverages [n-tier architecture](https://en.wikipedia.org/wiki/Multitier_architecture) and a dependency injection container.  The client and utility classes included in this download assist you in implementing the pattern.  This download includes a client implementation based on Autofac.  You should be able to implment similar functionality using other DI containers.  The Implementation section below describes the major components of AdaptiveClient.  The Concepts section describes how these components work together.

### *Implementation*
The fuctionality provided by AdaptiveClient comes primarily from the three classes shown below and their supporting classes:


    EndPointConfiguration

An `EndPointConfiguration` is like a connection string or a url but it includes some extra properties that are quite useful:

* **Name**: Name of the EndPoint: DevServer01, SlothInQA, etc.
* **API_Name**:  Name of the application or API exposed by the EndPoint: OurCompanyApp, xyz.com, etc.  Not the name of a contract or interface.
* **Preference**:  Number that allows ClientFactory to rank this EndPoint.  Lower numbers are ranked higher (more preferred).
* **EndPointType**:  May be one of the following:  InProcess, REST, WCF, ESB.  Assists ClientFactory in determining if the EndPoint is alive.
* **ConnectionString**:  Valid connection string OR url if pointing to an HTTP server.
* **Parameters**:  Not used at this time.
* **IsActive**:  Set this value to false to prevent reading this EndPoint from a config file.

&nbsp;
     
    ClientFactory

Given an interface and a collection of `EndPointConfiguration` objects,  `ClientFactory` will iterate over the EndPoints starting with the most preferred.  Upon finding an available EndPoint `ClientFactory` will return a suitable client that implements the desired interface.


    AutofacRegistrationHelper

AdaptiveClient is heavily dependent on the DI container.  Registering clients can be tricky but AutofacRegistrationHelper hides the complexity.  There are two simple steps to using `AutofacRegistrationHelper`:
 
1. Register the entire collection of `EndPointConfiguration` objects for an API or Application:

 ```csharp
AutofacRegistrationHelper registrationHelper = new AutofacRegistrationHelper(builder);
IEnumerable<IEndPointConfiguration> endPoints = ... // read endpoints from config file 
registrationHelper.RegisterEndPoints(endPoints);
```
 
2. A registration for each combination of client and `EndPointType` that is implemented by your application is required. These parameters are explained in greater detail in the Concepts section.  Three examples are shown below but only EndPointTypes you actually use are required.      

 ```csharp
 string apiName = "OurCompanyAPI";
 // client that communicates directly with the database
 registrationHelper.Register<AssemblyA.UsersService, IUsersService>(EndPointType.InProcess, apiName);
 // REST client
 registrationHelper.Register<AssemblyB.UsersService, IUsersService>(EndPointType.REST, apiName);
 // WCF client
 registrationHelper.Register<AssemblyC.UsersService, IUsersService>(EndPointType.WCF, apiName);
 ```
### *Concepts*



## Getting started



1. AdaptiveClient is designed to work with an n-tier architecture.  Make sure your application has clean separation of layers.  Generally this means you avoid putting business logic in your controllers, ViewModels, and code-behind classes.  Your business logic should reside entirely in your service layer.
2. Choose a DI container.  Autofac is an excellent choice and is supported by AdaptiveClient.
3. Define your EndPointConfigurations.  See the Demo application.
4. Its a good idea to put your Models and your Domain layer in their own assemblies.  Interfaces belong in the Domain layer (IOrdersService, IProductService, etc).
5. Create your Service layer in its own assembly (or assemblies).  Implement the interfaces defined your Domain layer.  Register your servers as shown in the Demo application.  
