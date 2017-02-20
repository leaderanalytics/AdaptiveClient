using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaderAnalytics.AdaptiveClient
{
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
}
