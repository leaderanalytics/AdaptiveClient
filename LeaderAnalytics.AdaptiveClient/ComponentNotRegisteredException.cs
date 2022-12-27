namespace LeaderAnalytics.AdaptiveClient;

public class ComponentNotRegisteredException : Exception
{
    public ComponentNotRegisteredException() : this(null)
    {
    }

    public ComponentNotRegisteredException(string msg) : this(msg,null)
    {

    }

    public ComponentNotRegisteredException(string msg, Exception inner) : base(msg, inner)
    {

    }
}
