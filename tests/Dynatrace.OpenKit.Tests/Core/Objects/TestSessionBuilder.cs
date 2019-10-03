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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;
using NSubstitute;

namespace Dynatrace.OpenKit.Core.Objects
{
    internal class TestSessionBuilder
    {
        private ILogger logger;
        private IOpenKitComposite parent;
        private IBeacon beacon;

        internal TestSessionBuilder()
        {
            logger = Substitute.For<ILogger>();
            parent = Substitute.For<IOpenKitComposite>();
            beacon = Substitute.For<IBeacon>();
        }

        internal TestSessionBuilder With(IBeacon beacon)
        {
             this.beacon = beacon;
            return this;
        }

        internal TestSessionBuilder With(ILogger logger)
        {
            this.logger = logger;
            return this;
        }

        internal TestSessionBuilder With(IOpenKitComposite parent)
        {
            this.parent = parent;
            return this;
        }

        internal ISessionInternals Build()
        {
            return new Session(
                logger,
                parent,
                beacon
            );
        }
    }
}