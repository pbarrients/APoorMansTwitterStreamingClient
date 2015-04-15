using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterClient.Infrastructure.Events.EventArguments;
using TwitterClient.Infrastructure.Models;
using TwitterClient.Infrastructure.Utility;

namespace TwitterClient.Infrastructure.Events
{
    class EventListener
    {
        public void Subscribe(TwitterStream s, TweetEventArgs e)
        {
            s.TweetReceivedEvent += new TwitterStream.TweetReceivedHandler(HeardIt);
        }
        private void HeardIt(TwitterStream s, TweetEventArgs e)
        {
            System.Console.WriteLine("Text: {0}", e.Tweet);
        }
    }
}
