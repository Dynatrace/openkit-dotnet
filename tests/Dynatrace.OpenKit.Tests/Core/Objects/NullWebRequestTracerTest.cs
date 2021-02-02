//
// Copyright 2018-2021 Dynatrace LLC
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

namespace Dynatrace.OpenKit.Core.Objects
{
    public class NullWebRequestTracerTest
    {
        [Test]
        public void TagReturnsEmptyString()
        {
            // given
            var target = NullWebRequestTracer.Instance;

            // when
            var obtained = target.Tag;

            // then
            Assert.That(obtained, Is.EqualTo(string.Empty));
        }

        [Test]
        public void SetResponseCodeReturnsSelf()
        {
            // given
            var target = NullWebRequestTracer.Instance;

            // when
            var obtained = target.SetResponseCode(200);

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesSentReturnsSelf()
        {
            // given
            var target = NullWebRequestTracer.Instance;

            // when
            var obtained = target.SetBytesSent(37);

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void SetBytesReceivedReturnsSelf()
        {
            // given
            var target = NullWebRequestTracer.Instance;

            // when
            var obtained = target.SetBytesReceived(73);

            // then
            Assert.That(obtained, Is.SameAs(target));
        }

        [Test]
        public void StartReturnsSelf()
        {
            // given
            var target = NullWebRequestTracer.Instance;

            // when
            var obtained = target.Start();

            // then
            Assert.That(obtained, Is.SameAs(target));
        }
    }
}