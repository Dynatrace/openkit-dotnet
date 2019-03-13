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
using System.Text;
using System.Threading;

namespace Dynatrace.OpenKit.Core
{
    public class DefaultLogger : ILogger
    {
        private readonly LogLevel logLevel;
        private readonly Action<string> writeLineAction;

        const string DateFormat = "O";

        public DefaultLogger(LogLevel logLevel)
        : this(logLevel, WriteLine)

        {
        }

        internal DefaultLogger(LogLevel logLevel, Action<string> writeLineAction)
        {
            this.logLevel = logLevel;
            this.writeLineAction = writeLineAction;
        }
        
        public bool IsErrorEnabled => LogLevel.ERROR.HasSameOrGreaterPriorityThan(logLevel);

        public bool IsWarnEnabled => LogLevel.WARN.HasSameOrGreaterPriorityThan(logLevel);

        public bool IsInfoEnabled => LogLevel.INFO.HasSameOrGreaterPriorityThan(logLevel);

        public bool IsDebugEnabled => LogLevel.DEBUG.HasSameOrGreaterPriorityThan(logLevel);

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

        public void Log(LogLevel logLevel, string message)
        {
            Log(logLevel, message, null);
        }

        public void Log(LogLevel logLevel, string message, Exception exception)
        {
            if (!logLevel.HasSameOrGreaterPriorityThan(this.logLevel))
            {
                return;
            }

            var logMessageBuilder = new StringBuilder(UTCTime)
                .Append(" ").Append(logLevel.Name())
                .Append(" [").Append(CurrentThreadName).Append("]")
                .Append(" ").Append(message);
            if (exception != null)
            {
                logMessageBuilder.Append(Environment.NewLine)
                    .Append(exception.ToString());
            }

            writeLineAction(logMessageBuilder.ToString());
        }

        public void Error(string message)
        {
            Log(LogLevel.ERROR, message);
        }

        public void Error(string message, Exception exception)
        {
            Log(LogLevel.ERROR, message, exception);
        }

        public void Warn(string message)
        {
            Log(LogLevel.WARN, message);
        }

        public void Info(string message)
        {
            Log(LogLevel.INFO, message);
        }

        public void Debug(string message)
        {
            Log(LogLevel.DEBUG, message);
        }

        private static void WriteLine(string text)
        {
#if !(WINDOWS_UWP || NETPCL4_5)
            Console.WriteLine(text);
#endif
        }
    }
}
