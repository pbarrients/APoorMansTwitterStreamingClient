using System;
using TwitterClient.Infrastructure.Config;
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

            var config = new StreamConfig()
            {
                ConsumerKey = customerKey,
                ConsumerSecret = customerSecret,
                AccessToken = accessToken,
                AccessSecret = accessTokenSecret,
                GeoOnly = true
            };
            var stream = new TwitterStreamClient(config);

            // subscribe to the event handler
            stream.TweetReceivedEvent += (sender, tweet) =>
            {
                if (tweet.Tweet.Id != 0)
                {
                    Console.WriteLine(tweet.Tweet.Id);
                }
            };

            stream.ExceptionReceived += (sender, exception) => Console.WriteLine(exception.TwitterException.ResponseMessage);

            stream.Start();

        }
    }
}
