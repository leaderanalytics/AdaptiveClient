using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LeaderAnalytics.AdaptiveClient
{
    public class Perimeter : IPerimeter
    {
        public string API_Name { get { return api_name; } }
        public IList<IEndPointConfiguration> EndPoints { get { return _endPoints; } }

        private string api_name;
        private IList<IEndPointConfiguration> _endPoints;

        public Perimeter(string api_name, IList<IEndPointConfiguration> endPoints)
        {
            this.api_name = api_name;
            _endPoints = endPoints.OrderBy(x => x.Preference).ToList();
        }
    }
}
