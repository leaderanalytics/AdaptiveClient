using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LeaderAnalytics.AdaptiveClient
{
    public class Perimeter : IPerimeter
    {
        public string API_Name { get { return application_name; } }
        public IList<IEndPointConfiguration> EndPoints { get { return _endPoints; } }

        private string application_name;
        private IList<IEndPointConfiguration> _endPoints;

        public Perimeter(string collection_name, IList<IEndPointConfiguration> endPoints)
        {
            this.application_name = collection_name;
            _endPoints = endPoints.OrderBy(x => x.Preference).ToList();
        }
    }
}
