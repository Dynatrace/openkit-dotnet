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

namespace Dynatrace.OpenKit.Protocol
{
    /// <summary>
    /// Provides additional parameters that will be appended when a server request is sent.
    /// </summary>
    public interface IAdditionalQueryParameters
    {
        /// <summary>
        /// Returns the current timestamp of the configuration received by the server.
        /// </summary>
        long ConfigurationTimestamp { get; }
    }
}