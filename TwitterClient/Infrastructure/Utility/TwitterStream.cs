using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwitterClient.Infrastructure.Enum;
using TwitterClient.Infrastructure.Events.EventArguments;
using TwitterClient.Infrastructure.Models;

namespace TwitterClient.Infrastructure.Utility
{
    public class TwitterStream : IDisposable, IStream
    {
        private string _consumerKey;
        private string _consumerSecret;
        private string _accessToken;
        private string _accessSecret;

        private const string streamUrl = "https://stream.twitter.com/1.1/statuses/filter.json";

        private List<string> trackKeywords;

        private Coordinates coordinates;

        private HttpWebRequest request;
        private HttpWebResponse response;
        private StreamReader responseStream;

        public event TweetReceivedHandler TweetReceivedEvent;
        public delegate void TweetReceivedHandler(TwitterStream s, TweetEventArgs e);

        public TwitterStream(string consumerKey, string consumerSecret, string accessToken, string accessSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
            _accessSecret = accessSecret;


        }

        private void SetTrackKeywords(List<string> words)
        {
            trackKeywords = words;
        }

        private void SetTrackKeywords(IEnumerable<string> words)
        {
            trackKeywords = words.ToList();
        }

        private void SetLocation(Coordinates coords)
        {
            coordinates = coords;
        }

        public void StartStream()
        {
            
            if (trackKeywords.Count > 0)
            {
                
            }

            if (coordinates.Latitude != null && coordinates.Longitude != null)
            {
                
            }


        }

        public Task StartStreamAsync()
        {
            // do web request

            throw new NotImplementedException();
        }

        public string QueryBuilder(List<string> keywords, Coordinates coords)
        {
            string theReturnString = string.Format("&track={0}&locations={1}", keywords, coordinates);
            if (theReturnString.IndexOf('&') == 0)
                theReturnString = theReturnString.Remove(0, 1).Replace("#", "%23");

            return theReturnString;
        }

        public void ResumeStream()
        {
            throw new NotImplementedException();
        }

        public void PauseStream()
        {
            throw new NotImplementedException();
        }

        public void StopStream()
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                request.Abort();
                responseStream.Close();
                responseStream = null;
                response.Close();
                response = null;
            }
        }

        
    }
}
