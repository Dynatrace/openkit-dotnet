using System;
using System.Collections.Generic;
using System.Text;

namespace Dynatrace.OpenKit.API
{
    /// <summary>
    /// This interface provides logging functionality to OpenKit. By subclassing OpenKit
    /// can make use of custom loggers.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a message with severity error
        /// </summary>
        /// <param name="message">the message to write to the log</param>
        void Error(string message);

        /// <summary>
        /// Logs a message with severity error. The provided exception is appended to the message
        /// </summary>
        /// <param name="message">the message to write to the log</param>
        /// <param name="exception">Optional. Exeption whose string will be attached to the log output</param>
        void Error(string message, Exception exception);

        /// <summary>
        /// Logs a message with severity warning
        /// </summary>
        /// <param name="message">the message to write to the log</param>
        void Warn(string message);

        /// <summary>
        /// Logs a message with severity info
        /// </summary>
        /// <param name="message">the message to write to the log</param>
        void Info(string message);

        /// <summary>
        /// Logs a message with severity debug
        /// </summary>
        /// <param name="message">the message to write to the log</param>
        void Debug(string message);

        /// <summary>
        /// Returns a flag if error messages are enabled
        /// </summary>
        /// <returns>Flag if errors are enabled</returns>
        bool ErrorEnabled { get; }

        /// <summary>
        /// Returns a flag if warning messages are enabled
        /// </summary>
        /// <returns>Flag if warning messages are enabled</returns>
        bool WarnEnabled { get; }

        /// <summary>
        /// Returns a flag if info messages are enabled
        /// </summary>
        /// <returns>Flag if info messages are enabled</returns>
        bool InfoEnabled { get; }

        /// <summary>
        /// Returns a flag if debug messages are enabled
        /// </summary>
        /// <returns>Flag if debug messages are enabled</returns>
        bool DebugEnabled { get; }

    }
}
