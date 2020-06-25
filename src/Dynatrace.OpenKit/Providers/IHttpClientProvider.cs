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

using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Providers
{
    /// <summary>
    /// Interface providing a method to create a new http client
    /// </summary>
    public interface IHttpClientProvider
    {
        /// <summary>
        /// Returns an HTTPClient based on the provided configuration
        /// </summary>
        /// <param name="configuration">the configuration used for creating the HTTP client</param>
        /// <returns></returns>
        IHttpClient CreateClient(IHttpClientConfiguration configuration);
    }
}
