
using System;
using System.Configuration;

namespace DDnsClient
{
    public class Log 
    {
        private static SimpleLogger _instance;

        private Log() 
        {
        }

        public static SimpleLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    string minLogLevel = ConfigurationManager.AppSettings["min-log-level"].ToString();

                    Enum.TryParse(minLogLevel.ToUpper(), out SimpleLogger.LogLevel logLevel);

                    _instance = new SimpleLogger(logLevel); 
                }
                return _instance;
            }
        }
    }
}
