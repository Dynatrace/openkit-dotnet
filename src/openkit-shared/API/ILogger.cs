using System;
using System.Collections.Generic;
using System.Text;

namespace Dynatrace.OpenKit.API
{
    public interface ILogger
    {
        /// <summary>
        /// Logs a message with severity error
        /// </summary>
        /// <param name="message">the message to write to the log</param>
        void error(string message);

        /// <summary>
        /// Logs a message with severity error. The provided exception is appended to the message
        /// </summary>
        /// <param name="message">the message to write to the log</param>
        /// <param name="exception">Optional. Exeption whose string will be attached to the log output</param>
        void error(string message, Exception exception);

        /// <summary>
        /// Logs a message with severity warning
        /// </summary>
        /// <param name="message">the message to write to the log</param>
        void warn(string message);

        /// <summary>
        /// Logs a message with severity info
        /// </summary>
        /// <param name="message">the message to write to the log</param>
        void info(string message);

        /// <summary>
        /// Logs a message with severity debug
        /// </summary>
        /// <param name="message">the message to write to the log</param>
        void debug(string message);

        /// <summary>
        /// Returns a flag if error messages are enabled
        /// </summary>
        /// <returns>Flag if errors are enabled</returns>
        bool isErrorEnabled();

        /// <summary>
        /// Returns a flag if warning messages are enabled
        /// </summary>
        /// <returns>Flag if warning messages are enabled</returns>
        bool isWarnEnabled();

        /// <summary>
        /// Returns a flag if info messages are enabled
        /// </summary>
        /// <returns>Flag if info messages are enabled</returns>
        bool isInfoEnabled();

        /// <summary>
        /// Returns a flag if debug messages are enabled
        /// </summary>
        /// <returns>Flag if debug messages are enabled</returns>
        bool isDebugEnabled();

    }
}
