using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using TwitterClient.Infrastructure.Events;
using TwitterClient.Infrastructure.Events.EventArguments;
using TwitterClient.Infrastructure.Models;
using TwitterClient.Infrastructure.OAuth;

namespace TwitterClient.Infrastructure.Utility
{
    public class TwitterStreamClient : OAuthBase, IDisposable
    {
        private string _consumerKey;
        private string _consumerSecret;
        private string _accessToken;
        private string _accessSecret;

        private HttpWebRequest webRequest = null;
        private HttpWebResponse webResponse = null;
        private StreamReader responseStream = null;

        public event TweetReceivedHandler TweetReceivedEvent;
        public delegate void TweetReceivedHandler(TwitterStreamClient s, TweetEventArgs e);

        public TwitterStreamClient(string consumerKey, string consumerSecret, string accessToken, string accessSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
            _accessSecret = accessSecret;
        }
        public void Start()
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

            try
            {
                while (true)
                {
                    try
                    {
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
                            Stream _twitterPost = webRequest.GetRequestStream();
                            _twitterPost.Write(_twitterTrack, 0, _twitterTrack.Length);
                            _twitterPost.Close();
                        }

                        webResponse = (HttpWebResponse)webRequest.GetResponse();
                        responseStream = new StreamReader(webResponse.GetResponseStream(), encode);

                        //Read the stream.
                        while (true)
                        {
                            jsonText = responseStream.ReadLine();                           

                            //Success
                            wait = 250;

                            var t = JsonConvert.DeserializeObject<Tweet>(jsonText, new JsonSerializerSettings());

                            this.Raise(TweetReceivedEvent, new TweetReceivedEventArgs(t));
                            //Write Status
                            //Console.Write(t);
                        }
                        //Abort is needed or responseStream.Close() will hang.
                        webRequest.Abort();
                        responseStream.Close();
                        responseStream = null;
                        webResponse.Close();
                        webResponse = null;
                    }
                    catch (WebException ex)
                    {
                        Console.WriteLine(ex.Message);
                        //Console.WriteLine(ex.InnerException);
                        
                        if (ex.Status == WebExceptionStatus.ProtocolError)
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
                                    wait = wait * 2;
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
                        Console.WriteLine(ex.Message);
                       
                    }
                    finally
                    {
                        if (webRequest != null)
                            webRequest.Abort();
                        if (responseStream != null)
                        {
                            responseStream.Close();
                            responseStream = null;
                        }

                        if (webResponse != null)
                        {
                            webResponse.Close();
                            webResponse = null;
                        }
                        Console.WriteLine("Waiting: " + wait);
                        Thread.Sleep(wait);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
                Console.WriteLine("Waiting: " + wait);
                Thread.Sleep(wait);
            }
        }




        public void Raise(TweetReceivedHandler handler, TweetEventArgs e)
        {
            if (handler != null)
            {
                handler(this, e);
            }
        }
        protected void RaiseMatchingTweetReceived(TweetReceivedEventArgs eventArgs)
        {
            Raise(TweetReceivedEvent,eventArgs);
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

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                webRequest.Abort();
                responseStream.Close();
                responseStream = null;
                webResponse.Close();
                webResponse = null;
            }
        }
    }
}
