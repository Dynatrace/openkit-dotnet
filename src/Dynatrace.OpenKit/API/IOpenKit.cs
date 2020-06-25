//
// Copyright 2018-2020 Dynatrace LLC
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
    ///  This interface provides basic OpenKit functionality, like creating a Session and shutting down OpenKit.
    /// </summary>
    public interface IOpenKit : IDisposable
    {
        /// <summary>
        ///  Waits until OpenKit is fully initialized.
        /// </summary>
        ///
        /// <remarks>
        /// The calling thread is blocked until OpenKit is fully initialized or until OpenKit is shut down using the
        /// <see cref="IOpenKit.Shutdown"/>@link #shutdown()} method.
        ///
        /// Be aware, if {@link com.dynatrace.openkit.OpenKitFactory} is wrongly configured, for example when creating an
        /// instance with an incorrect endpoint URL, then this method might hang indefinitely, unless <see cref="IOpenKit.Shutdown"/> is called.
        /// </remarks>
        ///
        /// <returns><code>true</code> if OpenKit is fully initialized, <code>false</code> when a shutdown request was made. </returns>
        bool WaitForInitCompletion();

        /// <summary>
        ///  Waits until OpenKit is fully initialized or the given timeout expired.
        /// </summary>
        ///
        /// <remarks>
        ///  The calling thread is blocked until OpenKit is fully initialized or until OpenKit is shut down using the
        ///  <see cref="IOpenKit.Shutdown"/> method or the timeout expired.
        ///
        ///  Be aware, if {@link com.dynatrace.openkit.OpenKitFactory} is wrongly configured, for example when creating an
        ///  instance with an incorrect endpoint URL, then this method might hang indefinitely, unless <see cref="IOpenKit.Shutdown"/> is called
        ///  or timeout expires.
        /// </remarks>
        ///
        /// <returns><code>true</code> if OpenKit is fully initialized, <code>false</code> when a shutdown request was made. </returns>
        bool WaitForInitCompletion(int timeoutMillis);

        /// <summary>
        ///  Returns whether OpenKit is initialized or not.
        /// </summary>
        /// <returns><code>true</code> if OpenKit is fully initialized, <code>false</code> if OpenKit still performs initialization.</returns>
        bool IsInitialized { get; }

        /// <summary>
        ///  Defines the version of the application.
        /// </summary>
        /// <param name="value">application version</param>
        [Obsolete("Use OpenKitBuilder to set ApplicationVersion")]
        string ApplicationVersion { set; }

        /// <summary>
        ///  Creates a Session instance which can then be used to create Actions.
        /// </summary>
        /// <param name="clientIpAddress">client IP address where this Session is coming from</param>
        /// <returns>Session instance to work with</returns>
        ISession CreateSession(string clientIpAddress);

        /// <summary>
        ///  Creates a Session instance which can then be used to create Actions.
        /// </summary>
        /// <remarks>
        /// This is similar to the method <see cref="CreateSession(string)"/>, except that
        /// the client's IP address is determined on the server side.
        /// </remarks>
        /// <returns>Session instance to work with</returns>
        ISession CreateSession();

        /// <summary>
        ///  Shuts down the OpenKit, ending all open Sessions and waiting for them to be sent.
        /// </summary>
        void Shutdown();
    }
}
