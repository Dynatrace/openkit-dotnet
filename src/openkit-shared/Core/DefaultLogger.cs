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

        public void Error(string message)
        {
            Console.WriteLine("[ERROR] " + message);
        }

        public void Error(string message, Exception exception)
        {
            Console.WriteLine("[ERROR] " + message + "[ " + exception.Message + "] " + "\n" + exception.StackTrace);
        }

        public void Warn(string message)
        {
            Console.WriteLine("[WARN ] " + message);
        }

        public void Info(string message)
        {
            Console.WriteLine("[INFO ] " + message);
        }

        public void Debug(string message)
        {
            Console.WriteLine("[DEBUG] " + message);
        }

        public bool IsErrorEnabled()
        {
            return true;
        }
 
        public bool IsWarnEnabled()
        {
            return true;
        }

        public bool IsInfoEnabled()
        {
            return verbose;
        }

        public bool IsDebugEnabled()
        {
            return verbose;
        }
    }
}
