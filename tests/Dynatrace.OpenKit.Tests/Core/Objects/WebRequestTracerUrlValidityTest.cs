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

using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Core.Objects
{
    public class WebRequestTracerUrlValidityTest
    {
       [Test]
        public void NullIsNotAValidUrlScheme()
        {
            // then
            Assert.That(WebRequestTracer.IsValidUrlScheme(null), Is.False);
        }

        [Test]
        public void AValidSchemeStartsWithALetter()
        {
            // when starting with lower case letter, then
            Assert.That(WebRequestTracer.IsValidUrlScheme("a://some.host"), Is.True);
        }

        [Test]
        public void AValidSchemeOnlyContainsLettersDigitsPlusPeriodOrHyphen()
        {
            // when the url scheme contains all allowed characters
            Assert.That(WebRequestTracer.IsValidUrlScheme("b1+Z6.-://some.host"), Is.True);
        }

        [Test]
        public void AValidSchemeAllowsUpperCaseLettersToo()
        {

            // when the url scheme contains all allowed characters
            Assert.That(WebRequestTracer.IsValidUrlScheme("Obp1e+nZK6i.t-://some.host"), Is.True);
        }

        [Test]
        public void AValidSchemeDoesNotStartWithADigit()
        {
            // when it does not start with a digit, then it's valid
            Assert.That(WebRequestTracer.IsValidUrlScheme("a1://some.host"), Is.True);
            // when it starts with a digit, then it's invalid
            Assert.That(WebRequestTracer.IsValidUrlScheme("1a://some.host"), Is.False);
        }

        [Test]
        public void ASchemeIsInvalidIfInvalidCharactersAreEncountered()
        {
            // then
            Assert.That(WebRequestTracer.IsValidUrlScheme("a()[]{}@://some.host"), Is.False);
        }

        [Test]
        public void AnUrlIsOnlySetInConstructorIfItIsValid()
        {
            // given
            var parent = Substitute.For<OpenKitComposite>();
            parent.ActionId.Returns(21);

            var target = CreateWebRequestTracer()
                .WithUrl("a1337://foo")
                .Build();

            // then
            Assert.That(target.Url, Is.EqualTo("a1337://foo"));
        }

        [Test]
        public void IfUrlIsInvalidTheDefaultValueIsUsed()
        {
            // given
            var parent = Substitute.For<OpenKitComposite>();
            parent.ActionId.Returns(42);

            var target = CreateWebRequestTracer()
                .WithUrl("foobar")
                .Build();

            // then
            Assert.That(target.Url, Is.EqualTo("<unknown>"));
        }

        [Test]
        public void UrlStoredDoesNotContainRequestParameters()
        {
            // given
            var target = CreateWebRequestTracer()
                .WithUrl("https://www.google.com/foo/bar?foo=bar&asdf=jklo")
                .Build();

            // then
            Assert.That(target.Url, Is.EqualTo("https://www.google.com/foo/bar"));
        }

        [Test]
        public void ANewlyCreatedWebRequestTracerDoesNotAttachToTheParent()
        {
            // given
            var mockParent = Substitute.For<IOpenKitComposite>();
            var target = CreateWebRequestTracer()
                .With(mockParent)
                .WithUrl("https://www.google.com/")
                .Build();

            // then
            Assert.That(target.Parent, Is.SameAs(mockParent));
            _ = mockParent.Received(1).ActionId;
        }

        private TestWebRequestTracerBuilder CreateWebRequestTracer()
        {
            return new TestWebRequestTracerBuilder();
        }
    }
}
