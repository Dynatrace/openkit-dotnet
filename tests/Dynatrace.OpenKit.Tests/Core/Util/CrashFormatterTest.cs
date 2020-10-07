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

using NUnit.Framework;
using System;

namespace Dynatrace.OpenKit.Core.Util
{
    [TestFixture]
    public class CrashFormatterTest
    {
        [Test]
        public void NameGivesTypeName()
        {
            // given
            var target = new CrashFormatter(new ArgumentException("some message"));

            // when
            var obtained = target.Name;

            // then
            Assert.That(obtained, Is.EqualTo(typeof(ArgumentException).ToString()));
        }

        [Test]
        public void ReasonGivesExceptionMessage()
        {
            // given
            const string exceptionMessage = "Austria erit in orbe ultima";
            var target = new CrashFormatter(new ArgumentException(exceptionMessage));

            // when
            var obtained = target.Reason;

            // then
            Assert.That(obtained, Is.EqualTo(exceptionMessage));
        }

        [Test]
        public void StackTraceGivesExceptionStackTrace()
        {
            // given
            Exception caught = null;
            try
            {
                CreateRecurisveCrash("foobar", 10);
            }
            catch (Exception e)
            {
                caught = e;
            }

            Assert.That(caught, Is.Not.Null);
            var target = new CrashFormatter(caught);

            // when
            var obtained = target.StackTrace;

            // then
            Assert.That(obtained, Is.EqualTo(target.StackTrace));
        }

        private static void CreateRecurisveCrash(string message, int callCount)
        {
            if (callCount > 0)
            {
                try
                {
                    CreateRecurisveCrash(message, callCount - 1);
                }
                catch (Exception e)
                {
                    // get in some nested exception
                    if (callCount % 2 == 0)
                    {
                        throw new ArgumentException("caught exception", e);
                    }
                }
            }
            throw new InvalidOperationException(message);
        }
    }
}
