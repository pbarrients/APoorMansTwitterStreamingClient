using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TwitterClient.Infrastructure.Config;
using TwitterClient.Infrastructure.Enum;
using TwitterClient.Infrastructure.Events.EventArguments;
using TwitterClient.Infrastructure.Models;
using TwitterClient.Infrastructure.OAuth;

namespace TwitterClient.Infrastructure.Utility
{
    public class TwitterStreamClient : OAuthBase
    {
        private const string FilteredUrl = @"https://stream.twitter.com/1.1/statuses/filter.json";

        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly string _accessToken;
        private readonly string _accessSecret;

        private readonly bool _isGeoEnabled;

        private readonly List<string> _keywords; 

        private HttpWebRequest _webRequest;
        private HttpWebResponse _webResponse;
        private StreamReader _responseStream;

        public event TweetReceivedHandler TweetReceivedEvent;

        public StreamState StreamState;

        public delegate void TweetReceivedHandler(TwitterStreamClient s, TweetEventArgs e);

        public event TwitterExceptionHandler ExceptionReceived;

        public delegate void TwitterExceptionHandler(TwitterStreamClient s, TwitterExceptionEventArgs e);

        public TwitterStreamClient(StreamConfig options)
        {
            _consumerKey = options.ConsumerKey;
            _consumerSecret = options.ConsumerSecret;
            _accessToken = options.AccessToken;
            _accessSecret = options.AccessSecret;
            _isGeoEnabled = options.GeoOnly;
            _keywords = options.TrackKeywords;
        }

        public async Task Start()
        {

            int wait = 250;
            string trackKeywords = "";

            if (_keywords.Count > 0 && _keywords != null)
            {
                trackKeywords = UrlEncode(String.Join(",", _keywords.ToArray()));
            }

            string followUserId = "";
            string locationCoord = ""; // url encoded string of coordinates

            if (_isGeoEnabled)
            {
                locationCoord = "-180%2C-90%2C180%2C90";
            }

            string postparameters = (trackKeywords.Length == 0 ? string.Empty : "&track=" + UrlEncode(trackKeywords)) +
                                    (followUserId.Length == 0 ? string.Empty : "&follow=" + followUserId) +
                                    (locationCoord.Length == 0 ? string.Empty : "&locations=" + locationCoord);

            
            if (!String.IsNullOrEmpty(postparameters))
            {
                if (postparameters.IndexOf('&') == 0)
                    postparameters = postparameters.Remove(0, 1).Replace("#", "%23");
            }

            try
            {
                _webRequest = (HttpWebRequest) WebRequest.Create(FilteredUrl);
                _webRequest.Timeout = -1;
                _webRequest.Headers.Add("Authorization", GetAuthHeader(FilteredUrl + "?" + postparameters));

                Encoding encode = Encoding.GetEncoding("utf-8");
                if (postparameters.Length > 0)
                {
                    _webRequest.Method = "POST";
                    _webRequest.ContentType = "application/x-www-form-urlencoded";

                    byte[] twitterTrack = encode.GetBytes(postparameters);

                    _webRequest.ContentLength = twitterTrack.Length;
                    var twitterPost = _webRequest.GetRequestStream();
                    twitterPost.Write(twitterTrack, 0, twitterTrack.Length);
                    twitterPost.Close();
                }

                _webRequest.BeginGetResponse(ar =>
                {
                    var req = (WebRequest) ar.AsyncState;

                    using (var response = req.EndGetResponse(ar))
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            while (!reader.EndOfStream)
                            {
                                try
                                {
                                    var jsonObj = JsonConvert.DeserializeObject<Tweet>(reader.ReadLine(),
                                    new JsonSerializerSettings());

                                    Raise(TweetReceivedEvent, new TweetEventArgs(jsonObj));
                                }
                                catch (JsonReaderException jsonEx)
                                {
                                    
                                }       
                            }
                        }
                    }

                }, _webRequest);
            }
            catch (WebException webEx)
            {
                var wRespStatusCode = ((HttpWebResponse) webEx.Response).StatusCode;
                Raise(ExceptionReceived,
                    new TwitterExceptionEventArgs(new TwitterException(wRespStatusCode, webEx.Message)));
                if (webEx.Status == WebExceptionStatus.ProtocolError)
                {
                    //-- From Twitter Docs -- 
                    //When a HTTP error (> 200) is returned, back off exponentially. 
                    //Perhaps start with a 10 second wait, double on each subsequent failure, 
                    //and finally cap the wait at 240 seconds. 
                    //Exponential Backoff
                    if (wait < 10000)
                        wait = 10000;
                    else
                    {
                        if (wait < 240000)
                            wait = wait*2;
                    }
                }
                else
                {
                    //-- From Twitter Docs -- 
                    //When a network error (TCP/IP level) is encountered, back off linearly. 
                    //Perhaps start at 250 milliseconds and cap at 16 seconds.
                    //Linear Backoff
                    if (wait < 16000)
                        wait += 250;
                }
            }
            catch (Exception ex)
            {
                // general catch block

                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (_webRequest != null)
                    _webRequest.Abort();
                if (_responseStream != null)
                {
                    _responseStream.Close();
                    _responseStream = null;
                }

                if (_webResponse != null)
                {
                    _webResponse.Close();
                    _webResponse = null;
                }
                Thread.Sleep(wait);
            }            
        }
        /// <summary>
        /// Raises Exception event
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="e"></param>
        public void Raise(TwitterExceptionHandler handler, TwitterExceptionEventArgs e)
        {
            if (handler != null)
            {
                handler(this, e);
            }
        }
        /// <summary>
        /// Raises tweet received event
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="e"></param>
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
