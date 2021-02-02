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
using System;
using System.Globalization;

namespace Dynatrace.OpenKit.Core.Util
{
    [TestFixture]
    public class CrashFormatterTest
    {
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
        private CultureInfo currentUICulture;
#endif

        [SetUp]
        public void Setup()
        {
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
            // Note: .NET Core 1.0/1.1 does not allow manipulating the thread's culture
            // Manipulating the culture for all threads might have negative impact
            currentUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
#endif
        }

        [TearDown]
        public void TearDown()
        {
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
            System.Threading.Thread.CurrentThread.CurrentUICulture = currentUICulture;
#endif
        }


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
            Assert.That(obtained, Is.EqualTo(caught.StackTrace.Replace(Environment.NewLine, CrashFormatter.NewLine)));
        }

        /// <summary>
        /// Method for providing a recursive crash.
        /// </summary>
        /// <remarks>
        /// Generics is artificially introduced to test serialization.
        /// </remarks>
        /// <typeparam name="MessageType">Any class.</typeparam>
        /// <param name="message">Message to pass to the exception</param>
        /// <param name="callCount">The number of recurisve calls</param>
        private static void CreateRecurisveCrash<MessageType>(MessageType message, int callCount) where MessageType : class
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
                    if (callCount == 2)
                    {
                        throw new ArgumentException("caught exception", e);
                    }
                }
            }
            throw new InvalidOperationException(message.ToString());
        }
    }
}
