using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DDnsClient
{
    internal class DDnsClient
    {
        string[] _sites;

        private readonly string _currIPFile;
        private SimpleLogger _log;

        internal DDnsClient(string[] sites)
        {
            _log = Log.Instance;
            _sites = sites;

            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            _currIPFile = Path.Combine(appDir, "currentip.dat");
        }

        internal string GetOldIP()
        {
            try
            {
                string text = File.ReadAllText(_currIPFile, Encoding.UTF8);
                string currIP = StripIP(text);
                return currIP;
            }
            catch (FileNotFoundException)
            {
                _log.Warning($"File {_currIPFile} not found, returning empty string to continue");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _log.Error($"Cannot open/read file {_currIPFile}");
                throw new Exception("Error getting old IP", ex); 
            }
        }

        private string StripIP(string input)
        {
            _log.Debug($"Stripping IP input: [{input}]");

            Regex ip = new Regex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
            MatchCollection result = ip.Matches(input);

            if (result.Count > 0)
            {
                return result[0].ToString();
            }
            else
            {
                _log.Debug($"IP not found in input");
                return string.Empty;
            }

        }

        internal string GetWanIP()
        {
            foreach (var site in _sites)
            {
                _log.Debug($"Getting IP from Site {site}");

                var resp = HttpGet(site);
                if (resp != null && resp.StatusCode == 200)
                {
                    string ip = StripIP(resp.Content);

                    if (ip != string.Empty)
                    {
                        _log.Debug($"IP found: {ip}");
                        return ip;
                    }
                }
            }
            _log.Error($"Cannot get valid wan IP from any site");
            return string.Empty;
        }

        private HttpResponse HttpGet(string uri)
        {
            try
            {
                HttpWebResponse response = null;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException we)
                {
                    // this will catch HTTP status from 400 to 50x
                    response = (HttpWebResponse)we.Response;
                }

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string sResponse = reader.ReadToEnd();

                HttpResponse resp = new HttpResponse() 
                            { StatusCode = (int)response.StatusCode, 
                            StatusDescr = response.StatusCode.ToString(),
                            Content = sResponse };
                
                _log.Debug($"Response Status: {resp}");
                _log.Debug($"Response Content: {resp.Content}");

                return resp;
            }
            catch (Exception ex)
            {
                _log.Error($"Error getting url: [{uri}]. Message: {ex}");
                return null;
            }
            
        }

        internal void UpdateIP(string updateURL, string hostName, string userName, string password, string newIP)
        {
            try
            {
                _log.Info($"Updating to new IP {newIP}");

                string url = updateURL.Replace("{hostname}", hostName);
                url = url.Replace("{username}", userName);
                url = url.Replace("{password}", password);
                url = url.Replace("{newip}", newIP);

                _log.Debug($"url: [{url}]");

                var resp = HttpGet(url);

                if (resp != null && resp.StatusCode == 200)
                {
                    _log.Debug($"server response: [{resp.Content}]");
                    File.WriteAllText(_currIPFile, newIP);
                    _log.Info("IP updated succesfully");
                }
                else
                {
                    _log.Error($"Cannot update IP, server response:\n[{resp.ToString()}]\n{resp.Content}");
                }
                return;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Updating IP", ex);
            }
            
        }
    }
}
