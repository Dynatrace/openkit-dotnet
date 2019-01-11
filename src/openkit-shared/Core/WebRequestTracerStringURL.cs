//
// Copyright 2018-2019 Dynatrace LLC
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

using Dynatrace.OpenKit.Protocol;
using System;

namespace Dynatrace.OpenKit.Core
{

    /// <summary>
    ///  Inherited class of WebRequestTracerBase which can be used for tracing and timing of a web request handled by any 3rd party HTTP Client.
    ///  Setting the Dynatrace tag to the OpenKit.WEBREQUEST_TAG_HEADER HTTP header has to be done manually by the user.
    /// </summary>
    public class WebRequestTracerStringURL : WebRequestTracerBase
    {

        // *** constructors ***

        public WebRequestTracerStringURL(Beacon beacon, Action action, string url) : base(beacon, action)
        {
            this.url = url;

            // separate query string from URL
            if (url != null)
            {
                this.url = url.Split(new Char[] { '?' })[0];
            }
        }

    }

}
