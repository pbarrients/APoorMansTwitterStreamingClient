using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TwitterClient.Infrastructure.Models
{
    public class Tweet
    {
        [DataMember]
        [JsonProperty("text")]
        public string Text;

        //[DataMember] public Geo geo;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Text);
            return sb.ToString();
        }
    }
}
