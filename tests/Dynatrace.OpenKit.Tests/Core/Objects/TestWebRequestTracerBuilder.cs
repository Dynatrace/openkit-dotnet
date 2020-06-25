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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;
using NSubstitute;

namespace Dynatrace.OpenKit.Core.Objects
{
    internal class TestWebRequestTracerBuilder
    {
        private ILogger logger;
        private IBeacon beacon;
        private IOpenKitComposite parentComposite;
        private string url;

        internal TestWebRequestTracerBuilder()
        {
            beacon = Substitute.For<IBeacon>();
            logger = Substitute.For<ILogger>();
            parentComposite = Substitute.For<OpenKitComposite>();

        }

        internal TestWebRequestTracerBuilder With(ILogger logger)
        {
            this.logger = logger;
            return this;
        }

        internal TestWebRequestTracerBuilder With(IBeacon beacon)
        {
            this.beacon = beacon;
            return this;
        }

        internal TestWebRequestTracerBuilder With(IOpenKitComposite parent)
        {
            parentComposite = parent;
            return this;
        }

        internal TestWebRequestTracerBuilder WithUrl(string tracerUrl)
        {
            url = tracerUrl;
            return this;
        }

        internal IWebRequestTracerInternals Build()
        {
            if (string.IsNullOrEmpty(url))
            {
                return new WebRequestTracer(
                    logger,
                    parentComposite,
                    beacon
                );
            }

            return new WebRequestTracer(
                logger,
                parentComposite,
                beacon,
                url
            );
        }
    }
}