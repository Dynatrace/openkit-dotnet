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
using System;
using System.Collections.Generic;
using System.Text;

namespace Dynatrace.OpenKit.Core
{
    class DefaultLoggerTest
    {
        [Test]
        public void DefaultLoggerWithVerboseOutputWritesErrorLevelMessages()
        {
            //given
            DefaultLogger log = new DefaultLogger(true);

            //then
            Assert.That(log.IsErrorEnabled, Is.True);
        }

        [Test]
        public void DefaultLoggerWithVerboseOutputWritesWarnLevelMessages()
        {
            //given
            DefaultLogger log = new DefaultLogger(true);

            //then
            Assert.That(log.IsWarnEnabled, Is.True);
        }

        [Test]
        public void DefaultLoggerWithVerboseOutputWritesInfoLevelMessages()
        {
            //given
            DefaultLogger log = new DefaultLogger(true);

            //then
            Assert.That(log.IsInfoEnabled, Is.True);
        }

        [Test]
        public void DefaultLoggerWithVerboseOutputWritesDebugLevelMessages()
        {
            //given
            DefaultLogger log = new DefaultLogger(true);

            //then
            Assert.That(log.IsDebugEnabled, Is.True);
        }

        [Test]
        public void DefaultLoggerWithoutVerboseOutputWritesErrorLevelMessages()
        {
            //given
            DefaultLogger log = new DefaultLogger(false);

            //then
            Assert.That(log.IsErrorEnabled, Is.True);
        }

        [Test]
        public void DefaultLoggerWithoutVerboseOutputWritesWarnLevelMessages()
        {
            //given
            DefaultLogger log = new DefaultLogger(false);

            //then
            Assert.That(log.IsWarnEnabled, Is.True);
        }

        [Test]
        public void DefaultLoggerWithoutVerboseOutputWritesInfoLevelMessages()
        {
            //given
            DefaultLogger log = new DefaultLogger(false);

            //then
            Assert.That(log.IsInfoEnabled, Is.False);
        }

        [Test]
        public void DefaultLoggerWithoutVerboseOutputWritesDebugLevelMessages()
        {
            //given
            DefaultLogger log = new DefaultLogger(false);

            //then
            Assert.That(log.IsDebugEnabled, Is.False);
        }
    }
}
