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

using System;

namespace Dynatrace.OpenKit.API
{

    /// <summary>
    ///  This interface provides functionality to create Actions in a Session.
    /// </summary>
    public interface ISession : IDisposable
    {

        /// <summary>
        ///  Enters an Action with a specified name in this Session.
        /// </summary>
        /// <remarks>
        /// If <paramref name="actionName"/> is <code>null</code> or an empty string,
        /// a <see cref="NullAction"/> is entered and therefore no action tracing happens.
        /// </remarks>
        /// <param name="actionName">name of the Action</param>
        /// <returns>Action instance to work with</returns>
        IRootAction EnterAction(string actionName);

        /// <summary>
        ///  Tags a session with the provided <code>userTag</code>
        /// </summary>
        /// <remarks>
        /// If <paramref name="userTag"/> is <code>null</code> or an empty string,
        /// no user identification is sent to the server.
        /// </remarks>
        /// <param name="userTag"></param>
        void IdentifyUser(string userTag);

        /// <summary>
        ///  Reports a crash with a specified error name, crash reason and a stacktrace.
        /// </summary>
        /// <param name="errorName">name of the error leading to the crash (e.g. Exception class)</param>
        /// <param name="reason">reason or description of that error</param>
        /// <param name="stacktrace">stacktrace leading to that crash</param>
        void ReportCrash(string errorName, string reason, string stacktrace);

        /// <summary>
        ///  Ends this Session and marks it as ready for immediate sending.
        /// </summary>
        void End();
    }

}
