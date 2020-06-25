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

using System;
using System.Threading;
using Dynatrace.OpenKit.Core.Util;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Util
{
    public class InterruptibleThreadSuspenderTest
    {
        [Test]
        public void SleepSuspendsCurrentThreadForSpecifiedAmountOfTime()
        {
            // given
            const int sleepTimeMillis = 5;
            var target = CreateInstance();

            // when
            var before = DateTime.UtcNow;
            target.Sleep(sleepTimeMillis);
            var sleptTimeInMillis = (long) (DateTime.UtcNow - before).TotalMilliseconds;

            // then
            Assert.That(sleptTimeInMillis, Is.GreaterThanOrEqualTo(sleepTimeMillis));
        }

        [Test]
        public void SleepingMultipleTimesWorks()
        {
            // given
            const int sleepTime = 1;
            const int numSleeps = 5;
            var target = CreateInstance();

            for (var i = 0; i < numSleeps; i++)
            {
                // when
                var before = DateTime.UtcNow;
                target.Sleep(sleepTime);
                var sleptTimeMillis = (long) (DateTime.UtcNow - before).TotalMilliseconds;

                // then
                Assert.That(sleptTimeMillis, Is.GreaterThanOrEqualTo(sleepTime));
            }
        }

        [Test]
        public void WakeUpInterruptsSleep()
        {
            // given
            const int sleepTime = 5000;
            var target = CreateInstance();
            var task = new ExecutableTask(target, sleepTime);

            // when
            var thread = ThreadSurrogate.Create("test-thread").Start(task.Process);
            target.WakeUp();

            thread.Join(sleepTime);

            // then
            Assert.That(task.SleptTime, Is.LessThan(sleepTime));
        }

        [Test]
        public void SleepReturnsImmediatelyAfterWakeUpWasCalledOnce()
        {
            // given
            const int sleepTime = 50;
            var target = CreateInstance();

            // when
            target.WakeUp();
            var before = DateTime.UtcNow;
            target.Sleep(sleepTime);
            var sleptTimeMillis = (long) (DateTime.UtcNow - before).TotalMilliseconds;

            // then
            Assert.That(sleptTimeMillis, Is.LessThan(sleepTime));
        }

#if !(NETCOREAPP1_0 || NETCOREAPP1_1 || WINDOWS_UWP || NETSTANDARD1_1)
        // Execute this test only on frameworks where Thread.Interrupt is available
        [Test]
        public void SleepReturnsFalseIfInterrupted()
        {
            // given
            const int sleepTime = 1000;
            var target = CreateInstance();
            var task = new ExecutableTask(target, sleepTime);

            // when
            var thread = ThreadSurrogate.Create("test").Start(task.Process);
            thread.Join(500);

            // then
            Assert.That(task.SleepOutCome, Is.EqualTo(0));
        }
#endif

        private InterruptibleThreadSuspender CreateInstance()
        {
            return new InterruptibleThreadSuspender();
        }

        private class ExecutableTask
        {
            private readonly InterruptibleThreadSuspender suspender;
            private readonly int sleepTimeMillis;

            private long sleptTime = -1;
            private long sleepOutcome = -1;

            public ExecutableTask(InterruptibleThreadSuspender suspender, int sleepTimeMillis)
            {
                this.suspender = suspender;
                this.sleepTimeMillis = sleepTimeMillis;
            }

            public void Process()
            {
                var now = DateTime.UtcNow;
                var sleptWell = suspender.Sleep(sleepTimeMillis);
                SleptTime = (long) (DateTime.UtcNow - now).TotalMilliseconds;
                SleepOutCome = sleptWell ? 1 : 0;
            }

            public long SleptTime
            {
                get => Interlocked.Read(ref sleptTime);
                private set => sleptTime = Interlocked.Exchange(ref sleptTime, value);
            }

            public long SleepOutCome
            {
                get => Interlocked.Read(ref sleepOutcome);
                set => Interlocked.Exchange(ref sleepOutcome, value);
            }
        }
    }
}