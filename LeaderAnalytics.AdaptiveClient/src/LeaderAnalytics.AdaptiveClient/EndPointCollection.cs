using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LeaderAnalytics.AdaptiveClient
{
    public class EndPointCollection : IEndPointCollection
    {
        public string Collection_Name { get { return collection_name; } }
        public IList<IEndPointConfiguration> EndPoints { get { return _endPoints; } }

        private string collection_name;
        private IList<IEndPointConfiguration> _endPoints;

        public EndPointCollection(string collection_name, IList<IEndPointConfiguration> endPoints)
        {
            this.collection_name = collection_name;
            _endPoints = endPoints.OrderBy(x => x.Preference).ToList();
        }
    }
}
