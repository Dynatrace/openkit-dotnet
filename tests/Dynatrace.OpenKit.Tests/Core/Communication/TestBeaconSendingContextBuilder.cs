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
using Dynatrace.OpenKit.Providers;

namespace Dynatrace.OpenKit.Core.Communication
{
    internal class TestBeaconSendingContextBuilder
    {
        private ILogger logger;
        private IHttpClientConfiguration httpClientConfig;
        private IHttpClientProvider httpClientProvider;
        private ITimingProvider timingProvider;
        private IInterruptibleThreadSuspender threadSuspender;
        private AbstractBeaconSendingState initState;

        internal TestBeaconSendingContextBuilder With(ILogger logger)
        {
            this.logger = logger;
            return this;
        }

        internal TestBeaconSendingContextBuilder With(IHttpClientConfiguration clientConfig)
        {
            httpClientConfig = clientConfig;
            return this;
        }

        internal TestBeaconSendingContextBuilder With(IHttpClientProvider clientProvider)
        {
            httpClientProvider = clientProvider;
            return this;
        }

        internal TestBeaconSendingContextBuilder With(ITimingProvider provider)
        {
            this.timingProvider = provider;
            return this;
        }

        internal TestBeaconSendingContextBuilder With(AbstractBeaconSendingState state)
        {
            initState = state;
            return this;
        }

        internal TestBeaconSendingContextBuilder With(IInterruptibleThreadSuspender suspender)
        {
            threadSuspender = suspender;
            return this;
        }

        internal BeaconSendingContext Build()
        {
            if (initState == null)
            {
                return new BeaconSendingContext(
                    logger,
                    httpClientConfig,
                    httpClientProvider,
                    timingProvider,
                    threadSuspender
                );
            }

            return new BeaconSendingContext(
                logger,
                httpClientConfig,
                httpClientProvider,
                timingProvider,
                threadSuspender,
                initState
            );
        }

    }
}