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

using System;
using System.Threading;
using Dynatrace.OpenKit.Core.Util;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Util
{
    public class ThreadSurrogateTest
    {
        private const long UpdatedValue = 10;

        private long someValue = -1;
        private ManualResetEvent threadFinishedEvent;

        [SetUp]
        public void SetUp()
        {
            threadFinishedEvent = new ManualResetEvent(false);
            ModifiableValue = -1;
        }

        [Test]
        public void CreateInstantiatesANewThreadSurrogate()
        {
            // given
            var target = ThreadSurrogate.Create("test");

            // then
            Assert.That(target, Is.Not.Null.And.InstanceOf<ThreadSurrogate>());
        }

        [Test]
        public void CreateSetsThreadName()
        {
            // given
            const string threadName = "test-thread";

            // when
            var target = ThreadSurrogate.Create(threadName);

            // then
            Assert.That(target.ThreadName, Is.EqualTo(threadName));
        }

        [Test]
        public void StartExecutesGivenThreadStart()
        {
            // given
            var target = ThreadSurrogate.Create("test");

            // when
            target.Start(ThreadStart);
            threadFinishedEvent.WaitOne(100);

            // then
            Assert.That(ModifiableValue, Is.EqualTo(UpdatedValue));
        }

        [Test]
        public void JoinOnANotStartedThreadDoesNothing()
        {
            // given
            var target = ThreadSurrogate.Create("test");

            // when
            target.Join(10);

            // then
            Assert.That(ModifiableValue, Is.EqualTo(-1));
        }

        [Test]
        public void AStartedThreadThrowsExceptionIfStartIsCalledAgain()
        {
            // given
            const string threadName = "test";
            var target = ThreadSurrogate.Create(threadName).Start(ThreadStart);

            // when
            Assert.Throws<InvalidOperationException>(() => target.Start(ThreadStart), $"Thread {threadName} already started.");
        }

        [Test]
        public void JoinWaitsForTheThreadToFinish()
        {
            // given
            var target = ThreadSurrogate.Create("test").Start(ThreadStart);

            // when
            target.Join(100);

            // then
            Assert.That(ModifiableValue, Is.EqualTo(UpdatedValue));
        }

        private void ThreadStart()
        {
            ModifiableValue = UpdatedValue;
            threadFinishedEvent.Set();
        }

        private long ModifiableValue
        {
            get => Interlocked.Read(ref someValue);
            set => Interlocked.Exchange(ref someValue, value);
        }
    }
}