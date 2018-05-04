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

using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Caching;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core
{
    public class WebRequestTracerStringURLTest
    {
        private Beacon beacon;
        private Action action;

        [SetUp]
        public void SetUp()
        {
            var logger = Substitute.For<ILogger>();
            beacon = new Beacon(logger,
                                new BeaconCache(),
                                new TestConfiguration(),
                                "127.0.0.1",
                                Substitute.For<IThreadIDProvider>(),
                                Substitute.For<ITimingProvider>());
            action = new RootAction(logger, beacon, "ActionName", new SynchronizedQueue<IAction>());
        }

        [Test]
        public void NullIsNotAValidURLScheme()
        {
            // then
            Assert.That(WebRequestTracerStringURL.IsValidURLScheme(null), Is.False);
        }

        [Test]
        public void AValidSchemeStartsWithALetter()
        {
            // when starting with lower case letter, then
            Assert.That(WebRequestTracerStringURL.IsValidURLScheme("a://some.host"), Is.True);
        }

        [Test]
        public void AValidSchemeOnlyContainsLettersDigitsPlusPeriodOrHyphen()
        {
            // when the url scheme contains all allowed characters
            Assert.That(WebRequestTracerStringURL.IsValidURLScheme("b1+Z6.-://some.host"), Is.True);
        }

        [Test]
        public void AValidSchemeAllowsUpperCaseLettersToo()
        {

            // when the url scheme contains all allowed characters
            Assert.That(WebRequestTracerStringURL.IsValidURLScheme("Obp1e+nZK6i.t-://some.host"), Is.True);
        }

        [Test]
        public void AValidSchemeDoesNotStartWithADigit()
        {
            // when it does not start with a digit, then it's valid
            Assert.That(WebRequestTracerStringURL.IsValidURLScheme("a1://some.host"), Is.True);
            // when it starts with a digit, then it's invalid
            Assert.That(WebRequestTracerStringURL.IsValidURLScheme("1a://some.host"), Is.False);
        }

        [Test]
        public void ASchemeIsInvalidIfInvalidCharactersAreEncountered()
        {
            // then
            Assert.That(WebRequestTracerStringURL.IsValidURLScheme("a()[]{}@://some.host"), Is.False);
        }

        [Test]
        public void AnURLIsOnlySetInConstructorIfItIsValid()
        {
            // given
            var target = new WebRequestTracerStringURL(beacon, action, "a1337://foo");

            // then
            Assert.That(target.URL, Is.EqualTo("a1337://foo"));
        }

        [Test]
        public void IfURLIsInvalidTheDefaultValueIsUsed()
        {
            // given
            var target = new WebRequestTracerStringURL(beacon, action, "foobar");

            // then
            Assert.That(target.URL, Is.EqualTo("<unknown>"));
        }
    }
}
