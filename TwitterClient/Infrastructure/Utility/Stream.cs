using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TwitterClient.Infrastructure.Events.EventArguments;
using TwitterClient.Infrastructure.Models;
using TwitterClient.Infrastructure.OAuth;

namespace TwitterClient.Infrastructure.Utility
{
    public class Stream : OAuthBase
    {
        private string _consumerKey;
        private string _consumerSecret;
        private string _accessToken;
        private string _accessSecret;

        private HttpWebRequest webRequest = null;
        private HttpWebResponse webResponse = null;
        private StreamReader responseStream = null;

        public event TweetReceivedHandler TweetReceivedEvent;
        public delegate void TweetReceivedHandler(Stream s, TweetEventArgs e);

        public Stream(string consumerKey, string consumerSecret, string accessToken, string accessSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
            _accessSecret = accessSecret;
        }

        public async Task Start()
        {
            //Twitter Streaming API
            string stream_url = "https://stream.twitter.com/1.1/statuses/filter.json";



            string trackKeywords = "twitter";
            string followUserId = "";
            string locationCoord = "";

            string postparameters = (trackKeywords.Length == 0 ? string.Empty : "&track=" + trackKeywords) +
                                    (followUserId.Length == 0 ? string.Empty : "&follow=" + followUserId) +
                                    (locationCoord.Length == 0 ? string.Empty : "&locations=" + locationCoord);

            if (!string.IsNullOrEmpty(postparameters))
            {
                if (postparameters.IndexOf('&') == 0)
                    postparameters = postparameters.Remove(0, 1).Replace("#", "%23");
            }

            int wait = 250;
            string jsonText = "";
            //Connect
            webRequest = (HttpWebRequest)WebRequest.Create(stream_url);
            webRequest.Timeout = -1;
            webRequest.Headers.Add("Authorization", GetAuthHeader(stream_url + "?" + postparameters));

            Encoding encode = Encoding.GetEncoding("utf-8");
            if (postparameters.Length > 0)
            {
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";

                byte[] _twitterTrack = encode.GetBytes(postparameters);

                webRequest.ContentLength = _twitterTrack.Length;
                var _twitterPost = webRequest.GetRequestStream();
                _twitterPost.Write(_twitterTrack, 0, _twitterTrack.Length);
                _twitterPost.Close();
            }

            //webResponse = (HttpWebResponse)webRequest.GetResponse();
            //responseStream = new StreamReader(webResponse.GetResponseStream(), encode);


            webRequest.BeginGetResponse(ar =>
            {
                var req = (WebRequest) ar.AsyncState;

                using (var response = req.EndGetResponse(ar))
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        while (!reader.EndOfStream)
                        {
                            //Console.WriteLine(reader.ReadLine());

                            var jsonObj = JsonConvert.DeserializeObject<Tweet>(reader.ReadLine(), new JsonSerializerSettings());
                            Raise(TweetReceivedEvent, new TweetEventArgs(jsonObj));
                        }
                    }
                }

            }, webRequest);

        }
        public void Raise(TweetReceivedHandler handler, TweetEventArgs e)
        {
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private string GetAuthHeader(string url)
        {
            string normalizedString;
            string normalizeUrl;
            string timeStamp = GenerateTimeStamp();
            string nonce = GenerateNonce();


            string oauthSignature = GenerateSignature(new Uri(url), _consumerKey, _consumerSecret, _accessToken, _accessSecret, "POST", timeStamp, nonce, out normalizeUrl, out normalizedString);


            // create the request header
            const string headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                                        "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                                        "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                                        "oauth_version=\"{6}\"";

            return string.Format(headerFormat,
                Uri.EscapeDataString(nonce),
                Uri.EscapeDataString(Hmacsha1SignatureType),
                Uri.EscapeDataString(timeStamp),
                Uri.EscapeDataString(_consumerKey),
                Uri.EscapeDataString(_accessToken),
                Uri.EscapeDataString(oauthSignature),
                Uri.EscapeDataString(OAuthVersion));
        }
    }
}
