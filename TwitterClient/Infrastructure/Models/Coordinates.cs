using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterClient.Infrastructure.Models
{
    public class Coordinates
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public Coordinates() { }

        /// <summary>
        /// Create coordinates with its longitude and latitude
        /// </summary>
        public Coordinates(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", Latitude, Longitude);
        }
    }
}
