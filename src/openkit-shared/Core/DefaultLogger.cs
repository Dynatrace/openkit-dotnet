//
// Copyright 2018 Dynatrace LLC
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

namespace Dynatrace.OpenKit.Core
{
    public class DefaultLogger : ILogger
    {
        private readonly bool verbose;

        const string DATEFORMAT = "O";

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
#if !WINDOWS_UWP
            Console.WriteLine(GetUTCTime() + " [ERROR] " + message);
#endif
        }

        public void Error(string message, Exception exception)
        {
#if !WINDOWS_UWP
            Console.WriteLine(GetUTCTime() + " [ERROR] " + message + Environment.NewLine + exception.ToString());
#endif
        }

        public void Warn(string message)
        {
#if !WINDOWS_UWP
            Console.WriteLine(GetUTCTime() + " [WARN ] " + message);
#endif
        }

        public void Info(string message)
        {
            if (IsInfoEnabled)
            {
#if !WINDOWS_UWP
                Console.WriteLine(GetUTCTime() + " [INFO ] " + message);
#endif
            }
        }

        public void Debug(string message)
        {
            if (IsDebugEnabled)
            {
#if !WINDOWS_UWP
                Console.WriteLine(GetUTCTime() + " [DEBUG] " + message);
#endif
            }
        }

        public bool IsErrorEnabled => true;

        public bool IsWarnEnabled => true;

        public bool IsInfoEnabled => verbose;

        public bool IsDebugEnabled => verbose;
    }
}
