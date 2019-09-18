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

using NUnit.Framework;

namespace Dynatrace.OpenKit.Providers
{
    public class DefaultSessionIdProviderTest
    {
        [Test]
        public void DefaultSessionIdProviderReturnsNonNegativeId()
        {
            // given
            var provider = new DefaultSessionIdProvider();

            // then
            Assert.That(provider.GetNextSessionId(), Is.GreaterThan(0));
        }

        [Test]
        public void DefaultSessionIdProviderReturnsConsecutiveIds()
        {
            // given
            var provider = new DefaultSessionIdProvider(int.MaxValue / 2);

            // when
            var sessionIdOne = provider.GetNextSessionId();
            var sessionIdTwo = provider.GetNextSessionId();

            // then
            Assert.That(sessionIdTwo, Is.EqualTo(sessionIdOne + 1));
        }

        [Test]
        public void AProviderInitializedWithMaxIntValueProvidesMinSessionIdValueAtNextCall()
        {
            //given
            var provider = new DefaultSessionIdProvider(int.MaxValue);

            //when
            var actual = provider.GetNextSessionId();

            //then
            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void AProviderInitializedWithZeroProvidesMinSessionIdValueAtNextCall()
        {
            //given
            var provider = new DefaultSessionIdProvider(0);

            //when
            var actual = provider.GetNextSessionId();

            //then
            Assert.That(actual, Is.EqualTo(1));
        }
    }
}
