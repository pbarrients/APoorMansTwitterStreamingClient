using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterClient.Infrastructure.Models;

namespace TwitterClient.Infrastructure.Helpers
{
    class Params
    {
        private Coordinates _coordinates;
        private List<string> _trackedKeywords = new List<string>();

        public Params(Coordinates location = null, List<string> keywords = null)
        {
            _coordinates = location;
            _trackedKeywords = keywords;
        }

        public override string ToString()
        {
            string location = _coordinates != null ? string.Format("&locations={0}", _coordinates.ToString()) : string.Empty;
            string keywords = !_trackedKeywords.IsEmpty()
                ? string.Format("&track={0}", string.Join(",", _trackedKeywords.ToArray())) : string.Empty;

            string allOfIt = string.Format("{0}{1}", location, keywords);

            if (allOfIt.IndexOf('&') == 0)
            {
                allOfIt = allOfIt.Remove(0, 1).Replace("#", "%23");
            }
            return "?" + allOfIt;
        }
    }
}
