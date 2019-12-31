using System;
using System.Configuration;

namespace DDnsClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = Log.Instance;

            try
            {
                log.Debug("Obtaining settings from config file");

                string updateurl = ConfigurationManager.AppSettings["updateurl"].ToString();
                string hostname = ConfigurationManager.AppSettings["hostname"].ToString();
                string username = ConfigurationManager.AppSettings["username"].ToString();
                string password = ConfigurationManager.AppSettings["password"].ToString();
                string sites = ConfigurationManager.AppSettings["sites"].ToString();

                var client = new DDnsClient(sites.Split(';'));

                string oldIP = client.GetOldIP();
                string newIP = client.GetWanIP();

                if (oldIP != newIP)
                {
                    if (newIP != string.Empty)
                    {
                        client.UpdateIP(updateurl, hostname, username, password, newIP);
                    }
                    else
                    {
                        log.Error($"Error getting Wan IP address. Last known IP address {oldIP}");
                    }
                }
                else
                {
                    log.Info($"No IP change, current IP: {oldIP}");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }

            log.Info("End process.");
#if DEBUG
            Console.WriteLine("press any key to exit");
            Console.ReadKey();
#endif
        }
    }
}
