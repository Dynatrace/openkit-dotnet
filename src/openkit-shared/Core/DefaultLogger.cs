//
// Copyright 2018-2019 Dynatrace LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using Dynatrace.OpenKit.API;
using System;
using System.Globalization;
using System.Threading;

namespace Dynatrace.OpenKit.Core
{
    public class DefaultLogger : ILogger
    {
        private readonly bool verbose;
        private readonly System.Action<string> writeLineAction;

        const string DateFormat = "O";

        public DefaultLogger(bool verbose)
        : this(verbose, WriteLine)

        {
        }

        internal DefaultLogger(bool verbose, System.Action<string> writeLineAction)
        {
            this.verbose = verbose;
            this.writeLineAction = writeLineAction;
        }
        
        public bool IsErrorEnabled => true;

        public bool IsWarnEnabled => true;

        public bool IsInfoEnabled => verbose;

        public bool IsDebugEnabled => verbose;

        private static string UTCTime => DateTime.UtcNow.ToString(DateFormat, CultureInfo.InvariantCulture);
        
        private string CurrentThreadName
        {
            get
            {
#if !(WINDOWS_UWP || NETPCL4_5)

                var threadName = Thread.CurrentThread.Name;
                if (!string.IsNullOrEmpty(threadName))
                {
                    return threadName;
                }
                return Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
#else
                return System.Threading.Tasks.Task.CurrentId?.ToString(CultureInfo.InvariantCulture) ?? "N/A";
#endif
            }
        }

        public void Error(string message)
        {
            writeLineAction(UTCTime + " ERROR [" + CurrentThreadName + "] " + message);
        }

        public void Error(string message, Exception exception)
        {
            writeLineAction(UTCTime + " ERROR [" + CurrentThreadName + "] " + message 
                + Environment.NewLine + exception.ToString());
        }

        public void Warn(string message)
        {
            writeLineAction(UTCTime + " WARN  [" + CurrentThreadName + "] " + message);
        }

        public void Info(string message)
        {
            if (IsInfoEnabled)
            {
                writeLineAction(UTCTime + " INFO  [" + CurrentThreadName + "] " + message);
            }
        }

        public void Debug(string message)
        {
            if (IsDebugEnabled)
            {
                writeLineAction(UTCTime + " DEBUG [" + CurrentThreadName + "] " + message);
            }
        }

        private static void WriteLine(string text)
        {
#if !(WINDOWS_UWP || NETPCL4_5)
            Console.WriteLine(text);
#endif
        }
    }
}
