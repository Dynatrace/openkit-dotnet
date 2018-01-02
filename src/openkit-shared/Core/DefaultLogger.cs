using Dynatrace.OpenKit.API;
using System;


namespace Dynatrace.OpenKit.Core
{
    public class DefaultLogger : ILogger
    {
        private readonly bool verbose;

        static readonly string DATEFORMAT = "yyyy-MM-dd'T'HH:mm:ss.fffzz'00'";

        public DefaultLogger(bool verbose)
        {
            this.verbose = verbose;
        }

        private static string GetUTCTime()
        {
            return DateTime.UtcNow.ToString(DATEFORMAT);
        }

        public void Error(string message)
        {
            Console.WriteLine(GetUTCTime() + " [ERROR] " + message);
        }

        public void Error(string message, Exception exception)
        {
            Console.WriteLine(GetUTCTime() + " [ERROR] " + message);
            Console.WriteLine(exception.ToString());
        }

        public void Warn(string message)
        {
            Console.WriteLine(GetUTCTime() + " [WARN ] " + message);
        }

        public void Info(string message)
        {
            if (InfoEnabled)
            {
                Console.WriteLine(GetUTCTime() + " [INFO ] " + message);
            }
        }

        public void Debug(string message)
        {
            if (DebugEnabled)
            {
                Console.WriteLine(GetUTCTime() + " [DEBUG] " + message);
            }
        }

        public bool ErrorEnabled => true;

        public bool WarnEnabled => true;

        public bool InfoEnabled => verbose;

        public bool DebugEnabled => verbose;
    }
}
