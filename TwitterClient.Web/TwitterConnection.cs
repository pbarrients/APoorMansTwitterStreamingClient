using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR;
using TwitterClient.Infrastructure.Config;
using TwitterClient.Infrastructure.Utility;
using TwitterClient.Web.Hubs;

namespace TwitterClient.Web
{
    public class TwitterConnection
    {
        // Consumer Keys + Secrets
        private string _consumerKey = ConfigurationManager.AppSettings.Get("consumerKey");
        private string _consumerSecret = ConfigurationManager.AppSettings.Get("consumerSecret");
        // Twitter OAuth Credentials
        private string _accessKey = ConfigurationManager.AppSettings.Get("accessToken");
        private string _accessToken = ConfigurationManager.AppSettings.Get("accessTokenSecret");

        private IHubContext _context;

        public TwitterConnection()
        {
            _context = GlobalHost.ConnectionManager.GetHubContext<TwitterHub>();
            var config = new StreamConfig()
            {
                ConsumerKey = _consumerKey,
                ConsumerSecret = _consumerSecret,
                AccessToken = _accessKey,
                AccessSecret = _accessToken,
                GeoOnly = true
            };
            var stream = new TwitterStreamClient(config);

            stream.TweetReceivedEvent += (sender, args) =>
            {
                Debug.WriteLine(args.Tweet.ToString());
                _context.Clients.All.broadcast(args.Tweet);
            };
            stream.Start();
           
        }
    }
}