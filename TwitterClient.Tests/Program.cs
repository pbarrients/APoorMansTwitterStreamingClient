using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterClient.Infrastructure.Utility;

namespace TwitterClient.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            string accessToken = "874229294-vTpHJDcl8K0I7Ae6H29Ezvpw5ZVsX3uT2wbtDNkD";
            string accessTokenSecret = "3XNHrHBanj3x2fOTCm53t7L6hd7NSDlLqWIgc24um3x83";
            string customerKey = "kO6JlIwLa4czaQSqvHXLFfOhb";
            string customerSecret = "rSbhwvMR0vq1UCpkztfl3PvazveNHCKg6879J8yd0kLu7Q0xSF";

            using (var stream = new TwitterStream(customerKey,customerSecret,accessToken,accessTokenSecret))
            {
                stream.StartStream();
            }

        }
    }
}
