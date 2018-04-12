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

using NUnit.Framework;
using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using Dynatrace.OpenKit.Core.Communication;
using System;

namespace Dynatrace.OpenKit.FrameworkSpecificTests.Core.Communication
{
    public class BeaconSendingContextTest
    {
        private OpenKitConfiguration config;
        private IHTTPClientProvider clientProvider;
        private ITimingProvider timingProvider;

        [SetUp]
        public void Setup()
        {
            config = new TestConfiguration();
            clientProvider = Substitute.For<IHTTPClientProvider>();
            timingProvider = Substitute.For<ITimingProvider>();
        }

        [Test]
        public void CanInterruptLongSleep()
        {
            // given
            var expected = 101717;
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);
            target.RequestShutdown();
            target.Sleep(expected);

            // then
#if !NETCOREAPP1_0
            // normal sleep as thread interrupt exception exists
            timingProvider.Received(1).Sleep(expected);
#else
            // no interrupt exception exists, therefore "sliced" sleep break after first iteration
            timingProvider.Received(1).Sleep(Arg.Any<int>());
            timingProvider.Received(1).Sleep(BeaconSendingContext.DEFAULT_SLEEP_TIME_MILLISECONDS);
#endif
        }

        [Test]
        public void CanSleepLonger()
        {
            // given
            var expected = 101717;
            var target = new BeaconSendingContext(config, clientProvider, timingProvider);
            target.Sleep(expected);

            // then
#if !NETCOREAPP1_0
            // normal sleep as thread interrupt exception exists
            timingProvider.Received(1).Sleep(expected);
            
#else
            // no interrupt exception exists, therefore "sliced" sleeps until total sleep amount
            var expectedCount = (int)Math.Ceiling(expected / (double)BeaconSendingContext.DEFAULT_SLEEP_TIME_MILLISECONDS);
            timingProvider.Received(expectedCount).Sleep(Arg.Any<int>());
            timingProvider.Received(expectedCount - 1).Sleep(BeaconSendingContext.DEFAULT_SLEEP_TIME_MILLISECONDS);
            timingProvider.Received(1).Sleep(expected % BeaconSendingContext.DEFAULT_SLEEP_TIME_MILLISECONDS);
#endif
        }

    }
}
