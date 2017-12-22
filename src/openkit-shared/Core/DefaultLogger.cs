using Dynatrace.OpenKit.API;
using System;


namespace Dynatrace.OpenKit.Core
{
    public class DefaultLogger : ILogger
    {
        private bool verbose;

        public DefaultLogger(bool verbose)
        {
            this.verbose = verbose;
        }

        public void error(string message)
        {
            Console.WriteLine("[ERROR] " + message);
        }

        public void error(string message, Exception exception)
        {
            Console.WriteLine("[ERROR] " + message + "[ " + exception.Message + "] " + "\n" + exception.StackTrace);
        }

        public void warn(string message)
        {
            Console.WriteLine("[WARN ] " + message);
        }

        public void info(string message)
        {
            Console.WriteLine("[INFO ] " + message);
        }

        public void debug(string message)
        {
            Console.WriteLine("[DEBUG] " + message);
        }

        public bool isErrorEnabled()
        {
            return true;
        }
 
        public bool isWarnEnabled()
        {
            return true;
        }

        public bool isInfoEnabled()
        {
            return verbose;
        }

        public bool isDebugEnabled()
        {
            return verbose;
        }
    }
}
