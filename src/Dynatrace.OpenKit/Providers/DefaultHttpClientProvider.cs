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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Core.Util;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Providers
{
    internal class DefaultHttpClientProvider : IHttpClientProvider
    {
        private readonly ILogger logger;
        private readonly IInterruptibleThreadSuspender threadSuspender;

        public DefaultHttpClientProvider(ILogger logger, IInterruptibleThreadSuspender threadSuspender)
        {
            this.logger = logger;
            this.threadSuspender = threadSuspender;
        }

        public IHttpClient CreateClient(IHttpClientConfiguration configuration)
        {
#if NET40 || NET35
            return new HttpClientWebClient(logger, configuration, threadSuspender); // HttpClient is not available in .NET 3.5 and 4.0
#else
            return new HttpClientHttpClient(logger, configuration, threadSuspender);
#endif
        }
    }
}
