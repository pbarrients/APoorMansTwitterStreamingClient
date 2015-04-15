using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TwitterClient.Infrastructure.Models
{
    [DataContract]
    public class Geo
    {
        [DataMember]
        public string type;
        [DataMember]
        public string[] coordinates;
    }
}
