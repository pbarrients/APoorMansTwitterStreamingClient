using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterClient.Infrastructure.Models;

namespace TwitterClient.Infrastructure.Events.EventArguments
{
    public class TweetEventArgs : EventArgs
    {
        public TweetEventArgs(Tweet tweet)
        {
            Tweet = tweet;
        }

        public Tweet Tweet { get; private set; }
    }
}
