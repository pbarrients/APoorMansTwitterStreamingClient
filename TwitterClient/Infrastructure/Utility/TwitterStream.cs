using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TwitterClient.Infrastructure.Enum;
using TwitterClient.Infrastructure.Events.EventArguments;
using TwitterClient.Infrastructure.Helpers;
using TwitterClient.Infrastructure.Models;
using TwitterClient.Infrastructure.OAuth;
using WebRequest = System.Net.WebRequest;

namespace TwitterClient.Infrastructure.Utility
{
    public class TwitterStream : OAuthBase, IDisposable, IStream
    {
        private string _consumerKey;
        private string _consumerSecret;
        private string _accessToken;
        private string _accessSecret;

        private const string streamUrl = "https://stream.twitter.com/1.1/statuses/filter.json";

        private List<string> trackKeywords;

        //private Coordinates coordinates;

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

        //private void SetLocation(Coordinates coords)
        //{
        //    coordinates = coords;
        //}

        public void StartStream()
        {
            int wait = 250;
            string json = "";
            
            var parameters = new Params();
            try
            {
                // execute infinitely
                while (true)
                {
                    try
                    {
                        //Connect
                        request = (HttpWebRequest)WebRequest.Create(streamUrl);
                        request.Timeout = -1;
                        request.Headers.Add("Authorization", GetAuthHeader(streamUrl + parameters));

                        Encoding encode = Encoding.GetEncoding("utf-8");
                        if (parameters.ToString().Length > 0)
                        {
                            request.Method = "POST";
                            request.ContentType = "application/x-www-form-urlencoded";

                            byte[] _twitterTrack = encode.GetBytes(parameters.ToString());

                            request.ContentLength = _twitterTrack.Length;
                            Stream _twitterPost = request.GetRequestStream();
                            _twitterPost.Write(_twitterTrack, 0, _twitterTrack.Length);
                            _twitterPost.Close();
                        }

                        response = (HttpWebResponse)request.GetResponse();
                        responseStream = new StreamReader(response.GetResponseStream(), encode);

                        //Read the stream.
                        while (true)
                        {
                            json = responseStream.ReadLine();

                            //Success
                            wait = 250;

                            var t = JsonConvert.DeserializeObject<Tweet>(json, new JsonSerializerSettings());
                            //Write Status
                            Console.Write(t);
                        }
                        //Abort is needed or responseStream.Close() will hang.
                        request.Abort();
                        responseStream.Close();
                        responseStream = null;
                        response.Close();
                        response = null;
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
                        if (request != null)
                            request.Abort();
                        if (responseStream != null)
                        {
                            responseStream.Close();
                            responseStream = null;
                        }

                        if (response != null)
                        {
                            response.Close();
                            response = null;
                        }
                        Console.WriteLine("Waiting: " + wait);
                        Thread.Sleep(wait);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Waiting: " + wait);
                Thread.Sleep(wait);
            }
            
        }


    

        public Task StartStreamAsync()
        {
            // do web request

            throw new NotImplementedException();
        }

        public string QueryBuilder(string keywords, string locationCoord)
        {
            string postparameters = (keywords.Length == 0 ? string.Empty : "&track=" + trackKeywords) +
                                    (locationCoord.Length == 0 ? string.Empty : "&locations=" + locationCoord);

            if (postparameters.IndexOf('&') == 0)
                postparameters = postparameters.Remove(0, 1).Replace("#", "%23");

            return postparameters;
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
