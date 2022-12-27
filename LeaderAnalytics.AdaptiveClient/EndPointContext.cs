namespace LeaderAnalytics.AdaptiveClient;

/*
    Why this class exits and how is it different from EndPointCache:
    This class is created once per LifetimeScope, EndPointCache is created once per Application.
    
    So why not just inject EndPointCache and resolve the current endpoint?  To do that we need to know
    the API_Name, to get the API_Name we need to know the type of client being resolved.  
    
    The interface being resolved (i.e. IUsersService) is not registered with Autofac.  
    If we want to get the current endpoint using the type being resolved than we must register the type being resolved 
    AND we must resolve the infrastructure necessary to use that type to determine the current endpoint.
    
    For example, ServiceDbContextOptions receives a function that returns an EndPointConfiguration.
    If we instead pass EndPointCache to ServiceDbContextOptions than we must also pass a function that
    will return the current API name, and we must also register and pass the interface being resolved.
    
    The design implications of registering and passing the interface being resolved are no different than
    what we are doing with CurrentEndPoint - we are just using a different object.  Using CurrentEndPoint 
    saves us the expense of passing around the infrastructure necessary to resolve the current endpoint from
    the interface being resolved.  
    
    Note also that there is no difference in terms of timing - ClientFactory must do its work before either is valid.
*/

public class EndPointContext
{
    public IEndPointConfiguration CurrentEndPoint { get; internal set; }
}
