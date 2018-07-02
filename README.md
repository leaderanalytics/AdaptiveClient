# AdaptiveClient

#### Library and pattern for creating a scalable, loosely coupled service layer.  Build interdependent services that are granular and testable.  Inject a single client that allows the application to access the entire service layer.  Transparently access services and APIs regardless of transport or provider.


```c#
public partial class MainWindow : Window
{
    private IAdaptiveClient<IUsersService> client;

    public MainWindow(IAdaptiveClient<IUsersService> client)
    {
        this.client = client;
    }

    public async Task<IActionResult> Login(int userID)
    {
        // AdaptiveClient will attempt to use the server you define as most preferred. 
        // Server may be SQL, MySQL, WCF, REST, etc. - AdaptiveClient will resolve the correct 
        // client and all dependencies.  Your application does not need to know.
        // If the request fails AdaptiveClient will begin an orderly fall back to any other 
        // server that can handle the request:

        User user = await client.TryAsync(x => x.GetUser(userID));
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


## What AdaptiveClient does
Using the traditional repository pattern you pass a connection string down to your data access layer when you need to make a database call.  If the call fails the application usually fails or is blocked.  Most applications are also unusable if the user has no database connectivity via the Local Area Network.  

AdaptiveClient works backwards relative to the traditional pattern.  Before making a service call the application first asks AdaptiveClient to locate a server that can handle the request.  Upon locating a server, AdaptiveClient resolves the components necessary to communicate with that server given it's transport protocol and data provider.  The process of resolving components is handled by the dependency injection container (Autofac).  The pattern and a few utility and helper classes are provided by AdaptiveClient.


## Who will benefit from using it
* `AdaptiveClient` is ideally targeted to organizations that need to give local users access to their APIs over a local area network but who also wish to expose their APIs to remote users.
* Developers who want to implement retry and/or fall back logic when making service calls.


## How it works
`AdaptiveClient` is a design pattern that leverages [n-tier architecture](https://en.wikipedia.org/wiki/Multitier_architecture) and a dependency injection container (Autofac).  The classes included in this download assist you in implementing the pattern.  In a nutshell, AdaptiveClient works by associating three keys with each connection string in your application.  These three keys are **API_Name**, **EndPointType**, and **ProviderName**.  You define the values for each of these keys.  You register each of your connection strings (or API URL's) with AdaptiveClient using these keys.  You also use the same keys to register implementations of your services.  When you make a service call AdaptiveClient will locate a server that can handle the call.  Using the keys associated with the connection string (or URL) of the selected server, AdaptiveClient will resolve the specific dependencies required to communicate with that server.

The functionality provided by `AdaptiveClient` comes primarily from the classes shown below and their supporting classes:


    EndPointConfiguration

An `EndPointConfiguration` (a.k.a EndPoint for short) is like a connection string or a URL but it includes some extra properties that are useful:

* **Name**: Name of the EndPoint: DevServer01, QASloth02, etc.
* **API_Name**:  Name of the application or API exposed by the EndPoint: OurCompanyApp, xyz.com, etc.  NOT the name of a contract or interface.
* **Preference**:  Number that allows ClientFactory to rank this EndPoint.  Lower numbers are ranked higher (more preferred).
* **EndPointType**:  May be one of the following:  InProcess, HTTP, WCF, ESB.  Assists ClientFactory in determining if the EndPoint is alive.  Multiple EndPointConfigurations of the same `EndPointType` may be defined for an API_Name.
* **ProviderName**: A string that further describes the connection string data provider. Examples are "MSSQL" or "MySQL". Also used as a key to resolve services that are specific to the indicated ProviderName.
* **ConnectionString**:  Valid connection string OR URL if pointing to a HTTP server.
* **Parameters**:  Not used at this time.
* **IsActive**:  Set this value to false to prevent AdaptiveClient fro using this `EndPointConfiguration`.

&nbsp;
    

    RegistrationHelper

`RegistrationHelper` hides the complexity of registering  `EndPointConfiguration` objects and clients with the DI container.  Usage is discussed in the Getting Started section.  


    ServiceManifestFactory

AdaptiveClient supports the concept of a Service Manifest which is basically a collection of services that can be injected into your ViewModel, Controller, etc.  A Service Manifest is defined as follows:

* Create an Interface:
````csharp
public interface ISFServiceManifest : IDisposable
{
    // Services defined by StoreFront API:
    IOrdersService OrdersService { get; }
    IProductsService ProductsService { get; }
    // more services here...
}
````
* Create an implementation that derives from ServiceMaifestFactory and implements your interface:
````csharp
public class SFServiceManifest : ServiceManifestFactory, ISFServiceManifest
{
    // Services defined by StoreFront API:
    public IOrdersService OrdersService { get => Create<IOrdersService>(); }
    public IProductsService ProductsService { get => Create<IProductsService>(); }
    // more services here...
}
```` 
* Inject `IAdaptiveClient<ISFServiceManifest>` into your class:

````csharp
public class OrdersModel : BasePageModel
{
    private IAdaptiveClient<ISFServiceManifest> serviceClient;

    public OrdersModel(IAdaptiveClient<ISFServiceManifest> serviceClient)
    {
        this.serviceClient = serviceClient;
    }

    private async Task GetOrders()
    {
        // All services defined on ISFServiceManifest are available here:
        Orders = await serviceClient.CallAsync(async x => await x.OrdersService.GetOrders());
        Products = await serviceClient.CallAsync(async x => await x.ProductsService.GetProducts());
        // etc...
    }
}
````

    AdaptiveClient

AdaptiveClient implements two methods you can use to call your services:  Try and Call.  These methods differ in how and when they identify a server as being available.

* **Call and CallAsync**
````csharp
var orders = await serviceClient.CallAsync(async x => await x.OrdersService.GetOrders());
````
The Call and CallAsync methods attempt to discover an available server the first time (only) the methods are called for each API_Name that is registered with AdaptiveClient.  To determine if a server is available AdaptiveClient resolves an implemenation of `IEndPointValidator` and calls `IsInterfaceAlive` (You can use the implementations of IEndPointValidator found in the [AdaptiveClient.Utilities](https://www.nuget.org/packages/AdaptiveClient.Utilities/) nuget package or create your own).  After an available server is identified AdaptiveClient makes no other attempt to identify available servers. If you use the Call or CallAsync method and the call to the server fails your application will fail.  The Call and CallAsync methods do not provide fallback capabilities.


* **Try and TryAsync**
````csharp
var orders = await serviceClient.TryAsync(async x => await x.OrdersService.GetOrders());
````
Like their names imply, Try and TryAsync execute your method inside a try block.  If the call fails, AddaptiveClient will attempt to fall back to another server that is registered for the API.  Servers are contacted in the order of the Preference property on the EndPointConfiguration class.  If AdaptiveClient runs out of connection strings it will throw an exception.

## How `AdaptiveClient` resolves a client from start to finish: 

![How AdaptiveClient resolves a service from start to finish](https://raw.githubusercontent.com/leaderanalytics/AdaptiveClient/master/LeaderAnalytics.AdaptiveClient/docs/AdaptiveClient2.png)



## Getting started

1.  Install Nuget packages:
    * [Autofac](https://www.nuget.org/packages/Autofac/)
    * [AdaptiveClient](https://www.nuget.org/packages/AdaptiveClient/)
    * [AdaptiveClient.EntityFrameworkCore](https://www.nuget.org/packages/AdaptiveClient.EntityFrameworkCore/) (optional)
    * [AdaptiveClient.Utilities](https://www.nuget.org/packages/AdaptiveClient.Utilities/) (optional)
2. Define keys - Define API_Name, EndPointType, and ProviderName as described above.  These keys should be defined in the Domain layer of your app.
3. Define Endpoints - Create a file called something like EndPoints.json to define your EndPointConfigurations.  See one of the demo projects for an example.
4. Register required components -  You will need to register your EndPoints, your services, and possibly one or more `IEndPointValidator` implementations.  If you use Entity Framework you will also need to register `DbContext` and `DbContextOptions`.  See the section below and the example application.
5. Register optional components - Registering a `ServiceManifest` is optional but recommended even if you have a small number of services. If you use EF migrations you should register one or more `MigrationContexts`.  Use a `MigrationContext` to easily drop and re-create your database and apply migrations as necessary.  You may also want to register a `DatabaseInitalizer` if you seed your database when it is created or when a migration is applied.


#### Using `RegistrationHelper`
The [Zamagon demo](https://github.com/leaderanalytics/AdaptiveClient.EntityFramework.Zamagon) is written to run against both MySQL and MSSQL databases.  The code below is taken from that project and shows how services and Entity Framework objects are registered.

AdaptiveClient supports the concept of Modules that are similar to the object of the same name in Autofac.
The code below is taken from the Startup class of a dotnet core web application:
````csharp
// This method gets called by the runtime. Use this method to add services to the container.
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    // Other services registered here...

    // Autofac & AdaptiveClient
    IEnumerable<IEndPointConfiguration> endPoints = EndPointUtilities.LoadEndPoints("EndPoints.json");
    ContainerBuilder builder = new ContainerBuilder();
    builder.Populate(services);
    builder.RegisterModule(new LeaderAnalytics.AdaptiveClient.EntityFrameworkCore.AutofacModule());
    RegistrationHelper registrationHelper = new RegistrationHelper(builder);

    registrationHelper
        .RegisterEndPoints(endPoints)  // endPoints must be registered first
        .RegisterModule(new Zamagon.Services.Common.AdaptiveClientModule())
        .RegisterModule(new Zamagon.Services.BackOffice.AdaptiveClientModule())
        .RegisterModule(new Zamagon.Services.StoreFront.AdaptiveClientModule());

            
    var container = builder.Build();
    IDatabaseUtilities databaseUtilities = container.Resolve<IDatabaseUtilities>();
            
    // Create all databases or apply migrations
    foreach (IEndPointConfiguration ep in endPoints.Where(x => x.EndPointType == EndPointType.DBMS))
        databaseUtilities.CreateOrUpdateDatabase(ep).Wait();

    return container.Resolve<IServiceProvider>();
}
````
The following is an example of how services might registered for an app that reads MSSQL, MySQL, or calls a WebAPI server.  This code is also take from the [Zamagon demo](https://github.com/leaderanalytics/AdaptiveClient.EntityFramework.Zamagon).  

````csharp
public class AdaptiveClientModule : IAdaptiveClientModule
{
    public void Register(RegistrationHelper registrationHelper)
    {
        // --- StoreFront Services ---

        registrationHelper

        // MSSQL
        .RegisterService<StoreFront.MSSQL.OrdersService, IOrdersService>(EndPointType.DBMS, API_Name.StoreFront, DataBaseProviderName.MSSQL)
        .RegisterService<StoreFront.MSSQL.ProductsService, IProductsService>(EndPointType.DBMS, API_Name.StoreFront, DataBaseProviderName.MSSQL)
            
        // MySQL
        .RegisterService<StoreFront.MySQL.OrdersService, IOrdersService>(EndPointType.DBMS, API_Name.StoreFront, DataBaseProviderName.MySQL)
        .RegisterService<StoreFront.MySQL.ProductsService, IProductsService>(EndPointType.DBMS, API_Name.StoreFront, DataBaseProviderName.MySQL)

        // WebAPI
        .RegisterService<StoreFront.WebAPI.OrdersService, IOrdersService>(EndPointType.HTTP, API_Name.StoreFront, DataBaseProviderName.WebAPI)
        .RegisterService<StoreFront.WebAPI.ProductsService, IProductsService>(EndPointType.HTTP, API_Name.StoreFront, DataBaseProviderName.WebAPI)

        // DbContexts
        .RegisterDbContext<Database.Db>(API_Name.StoreFront)

        // Migration Contexts
        .RegisterMigrationContext<Database.Db_MSSQL>(API_Name.StoreFront, DataBaseProviderName.MSSQL)
        .RegisterMigrationContext<Database.Db_MySQL>(API_Name.StoreFront, DataBaseProviderName.MySQL)

        // Database Initializers
        .RegisterDatabaseInitializer<SFDatabaseInitializer>(API_Name.StoreFront, DataBaseProviderName.MSSQL)
        .RegisterDatabaseInitializer<SFDatabaseInitializer>(API_Name.StoreFront, DataBaseProviderName.MySQL) 

        // Service Manifests
        .RegisterServiceManifest<SFServiceManifest, ISFServiceManifest>(EndPointType.DBMS, API_Name.StoreFront, DataBaseProviderName.MSSQL)
        .RegisterServiceManifest<SFServiceManifest, ISFServiceManifest>(EndPointType.DBMS, API_Name.StoreFront, DataBaseProviderName.MySQL)
        .RegisterServiceManifest<SFServiceManifest, ISFServiceManifest>(EndPointType.HTTP, API_Name.StoreFront, DataBaseProviderName.WebAPI);
    }
}
````

 

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

