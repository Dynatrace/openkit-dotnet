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
using System.Collections.Generic;

namespace Dynatrace.OpenKit.API.HTTP
{
    /// <summary>
    /// Interface representing an HTTP request.
    /// </summary>
    public interface IHttpRequest
    {
        /// <summary>
        /// Gets the HTTP request <see cref="Uri"/>.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Gets the HTTP request method.
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Gets a map containing the request headers and their values.
        /// <para>
        /// The returned Dictionary is a copy and manipulations to it
        /// won't be reflected in the underlying data structure.
        /// </para>
        /// </summary>
        Dictionary<string, List<string>> Headers { get; }

        /// <summary>
        /// Tests if the request header is existent in the HTTP request.
        /// </summary>
        /// <param name="name">The header name to test for existence</param>
        /// <returns></returns>
        bool ExistsHeader(string name);

        /// <summary>
        /// Gets the header's values.
        /// </summary>
        /// <param name="name">The header name for which to retrieve the values</param>
        /// <returns></returns>
        List<string> GetHeader(string name);

        /// <summary>
        /// Adds the header and its value into the HTTP request headers.
        /// <para>
        /// If the header <see cref="ExistsHeader(string)">already exists</see>
        /// the value is appended.
        /// </para>
        /// </summary>
        /// <remarks>
        /// OpenKit .NET treats certain headers as reserved for compatibility reasons
        /// with OpenKit Java. 
        /// Trying to set one of the following restricted headers will return immediately.
        /// <list type="bullet">
        /// <item><description><c>Access-Control-Request-Headers</c></description></item>
        /// <item><description><c>Access-Control-Request-Method</c></description></item>
        /// <item><description><c>Connection</c></description></item>
        /// <item><description><c>Content-Length</c></description></item>
        /// <item><description><c>Content-Transfer-Encoding</c></description></item>
        /// <item><description><c>Host</c></description></item>
        /// <item><description><c>Keep-Alive</c></description></item>
        /// <item><description><c>Origin</c></description></item>
        /// <item><description><c>Trailer</c></description></item>
        /// <item><description><c>Transfer-Encoding</c></description></item>
        /// <item><description><c>Upgrade</c></description></item>
        /// <item><description><c>Via</c></description></item>
        /// </list>
        /// </remarks>
        /// <remarks>
        /// For .NET 3.5 and 4.0 the .NET framework adds a list of restricted headers
        /// https://docs.microsoft.com/en-us/dotnet/api/system.net.webheadercollection?view=netframework-4.0
        /// 
        /// From this list it is only possible to add
        /// <list type="bullet">
        /// <item><description><c>Referer</c></description></item>
        /// <item><description><c>User-Agent</c></description></item>
        /// </list>
        /// </remarks>
        /// <param name="name">The name of the HTTP header to add</param>
        /// <param name="value">The value of the HTTP header to add</param>
        void AddHeader(string name, string value);

        /// <summary>
        /// Removes the header from HTTP request headers.
        /// </summary>
        /// <remarks>
        /// OpenKit .NET treats certain headers as reserved for compatibility reasons
        /// with OpenKit Java. 
        /// Trying to remove one of the following restricted headers will return immediately.
        /// <list type="bullet">
        /// <item><description><c>Access-Control-Request-Headers</c></description></item>
        /// <item><description><c>Access-Control-Request-Method</c></description></item>
        /// <item><description><c>Connection</c></description></item>
        /// <item><description><c>Content-Length</c></description></item>
        /// <item><description><c>Content-Transfer-Encoding</c></description></item>
        /// <item><description><c>Host</c></description></item>
        /// <item><description><c>Keep-Alive</c></description></item>
        /// <item><description><c>Origin</c></description></item>
        /// <item><description><c>Trailer</c></description></item>
        /// <item><description><c>Transfer-Encoding</c></description></item>
        /// <item><description><c>Upgrade</c></description></item>
        /// <item><description><c>Via</c></description></item>
        /// </list>
        /// </remarks>
        /// <remarks>
        /// For .NET 3.5 and 4.0 the .NET framework adds a list of restricted headers
        /// https://docs.microsoft.com/en-us/dotnet/api/system.net.webheadercollection?view=netframework-4.0
        /// 
        /// From this list it is only possible to remove
        /// <list type="bullet">
        /// <item><description><c>Referer</c></description></item>
        /// <item><description><c>User-Agent</c></description></item>
        /// </list>
        /// </remarks>
        /// <param name="name">The name of the HTTP header to remove</param>
        void RemoveHeader(string name);
    }
}
