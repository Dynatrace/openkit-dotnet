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

using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core
{
#if NET40 || NET35

    /// <summary>
    ///  Inherited class of WebRequestTracerBase which can be used for tracing and timing of a web request provided via an HttpClient.
    /// </summary>
    public class WebRequestTracerWebClient : WebRequestTracerBase
    {

        // *** constructors ***

        // creates web request tracer with a System.Net.Http.HttpClient
        public WebRequestTracerWebClient(Beacon beacon, Action action, System.Net.WebClient webClient) : base(beacon, action)
        {
            if (webClient != null)
            {
                SetTagOnConnection(webClient);

                this.url = webClient.BaseAddress;
            }
        }

        // *** private methods ***

        // set the Dynatrace tag on the provided URLConnection
        private void SetTagOnConnection(System.Net.WebClient webClient)
        {
            // check if header is already set
            if (webClient.Headers.Get(OpenKitFactory.WEBREQUEST_TAG_HEADER) == null)
            {
                // if not yet set -> set it now
                webClient.Headers.Add(OpenKitFactory.WEBREQUEST_TAG_HEADER, Tag);
            }
        }

    }
#endif
}
