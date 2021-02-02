//
// Copyright 2018-2021 Dynatrace LLC
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

using System;
using System.Linq;

namespace Dynatrace.OpenKit.API
{
    /// <summary>
    /// Log level enumeration.
    /// </summary>
    /// <remarks>
    /// This level is used to specify the priority of a log event.
    /// </remarks>
    public enum LogLevel
    {
        DEBUG = 0,
        INFO = 10,
        WARN = 20,
        ERROR = 30
    }

    /// <summary>
    /// Extension method class to extend the <see cref="LogLevel"/> enum.
    /// </summary>
    public static class LogLevelExtensions
    {
        /// <summary>
        /// Maximum log level name length
        /// </summary>
        private static readonly int MaxLogLevelNameLength = Enum.GetNames(typeof(LogLevel)).Max(name => name.Length);

        /// <summary>
        /// Test if the <c>this</c> argument has same or higher priority (numerical value).
        /// </summary>
        /// <param name="lhs">Left hand side of the comparision.</param>
        /// <param name="rhs">Right hand side of the comparison.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="lhs"/> is greater than or equal to <paramref name="rhs"/>, <c>false</c> otherwise.
        /// </returns>
        public static bool HasSameOrGreaterPriorityThan(this LogLevel lhs, LogLevel rhs)
        {
            return lhs >= rhs;
        }

        /// <summary>
        /// Get the LogLevel's name.
        /// </summary>
        /// <param name="logLevel">Log level's name.</param>
        /// <returns>Log level's name.</returns>
        public static string Name(this LogLevel logLevel)
        {
            return Enum.GetName(typeof(LogLevel), logLevel).PadRight(MaxLogLevelNameLength);
        }
    }
}
