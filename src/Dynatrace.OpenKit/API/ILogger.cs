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

namespace Dynatrace.OpenKit.API
{
    /// <summary>
    /// This interface provides logging functionality to OpenKit. By subclassing OpenKit
    /// can make use of custom loggers.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log an event with given level and message
        /// </summary>
        /// <param name="logLevel">the log event level of the log entry</param>
        /// <param name="message">the message to write to the log</param>
        void Log(LogLevel logLevel, string message);

        /// <summary>
        /// Log an event with given level, message and exception.
        /// </summary>
        /// <param name="logLevel">the log event level of the log entry</param>
        /// <param name="message">the message to write to the log</param>
        /// <param name="exception">Optional. Exeption whose string will be attached to the log output</param>
        void Log(LogLevel logLevel, string message, Exception exception);

        /// <summary>
        /// Logs a message with severity error
        /// </summary>
        /// <remarks>
        /// This is a convenience method for <c>Log(LogLevel.ERROR, message)</c>
        /// </remarks>
        /// <param name="message">the message to write to the log</param>
        void Error(string message);

        /// <summary>
        /// Logs a message with severity error. The provided exception is appended to the message
        /// </summary>
        /// <remarks>
        /// This is a convenience method for <c>Log(LogLevel.ERROR, message, exception)</c>
        /// </remarks>
        /// <param name="message">the message to write to the log</param>
        /// <param name="exception">Optional. Exeption whose string will be attached to the log output</param>
        void Error(string message, Exception exception);

        /// <summary>
        /// Logs a message with severity warning
        /// </summary>
        /// <remarks>
        /// This is a convenience method for <c>Log(LogLevel.WARN, message, exception)</c>
        /// </remarks>
        /// <param name="message">the message to write to the log</param>
        void Warn(string message);

        /// <summary>
        /// Logs a message with severity info
        /// </summary>
        /// <remarks>
        /// This is a convenience method for <c>Log(LogLevel.INFO, message, exception)</c>
        /// </remarks>
        /// <param name="message">the message to write to the log</param>
        void Info(string message);

        /// <summary>
        /// Logs a message with severity debug
        /// </summary>
        /// <remarks>
        /// This is a convenience method for <c>Log(LogLevel.DEBUG, message, exception)</c>
        /// </remarks>
        /// <param name="message">the message to write to the log</param>
        void Debug(string message);

        /// <summary>
        /// Returns a flag if error messages are enabled
        /// </summary>
        /// <returns>Flag if errors are enabled</returns>
        bool IsErrorEnabled { get; }

        /// <summary>
        /// Returns a flag if warning messages are enabled
        /// </summary>
        /// <returns>Flag if warning messages are enabled</returns>
        bool IsWarnEnabled { get; }

        /// <summary>
        /// Returns a flag if info messages are enabled
        /// </summary>
        /// <returns>Flag if info messages are enabled</returns>
        bool IsInfoEnabled { get; }

        /// <summary>
        /// Returns a flag if debug messages are enabled
        /// </summary>
        /// <returns>Flag if debug messages are enabled</returns>
        bool IsDebugEnabled { get; }

    }
}
