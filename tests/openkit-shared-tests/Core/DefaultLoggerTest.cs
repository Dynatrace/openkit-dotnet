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
using System.Text.RegularExpressions;

namespace Dynatrace.OpenKit.Core
{
    public class DefaultLoggerTest
    {
        private const string LoggerDateTimePattern = "\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}.\\d{7}Z";

        private List<string> logOutputLines;
        private Action<string> writeLineAction;

        [SetUp]
        public void SetUp()
        {
            logOutputLines = new List<string>();
            writeLineAction = line => logOutputLines.Add(line);
        }

        [Test]
        public void ErrorLogsAppropriateMessage()
        {
            //given
            var target = new DefaultLogger(true, writeLineAction);

            // when
            target.Error("Error message");

            // then
            Assert.That(logOutputLines.Count, Is.EqualTo(1));
            Assert.That(Regex.IsMatch(logOutputLines[0], $"^{LoggerDateTimePattern} ERROR \\[.+?\\] Error message$"), Is.True);
        }

        [Test]
        public void ErrorWithStacktraceLogsAppropriateMessage()
        {
            //given
            var exception = new Exception("test exception");
            var target = new DefaultLogger(true, writeLineAction);

            // when
            target.Error("Error message", exception);

            // then
            Assert.That(logOutputLines.Count, Is.EqualTo(1));
            Assert.That(Regex.IsMatch(logOutputLines[0], $"^{LoggerDateTimePattern} ERROR \\[.+?\\] Error message(\n|\r|\r\n){Regex.Escape(exception.ToString())}$",
                RegexOptions.Singleline), Is.True);
        }

        [Test]
        public void WarningLogsAppropriateMessage()
        {
            //given
            var target = new DefaultLogger(true, writeLineAction);

            // when
            target.Warn("Warning message");

            // then
            Assert.That(logOutputLines.Count, Is.EqualTo(1));
            Assert.That(Regex.IsMatch(logOutputLines[0], $"^{LoggerDateTimePattern} WARN  \\[.+?\\] Warning message$"), Is.True);
        }

        [Test]
        public void InfoLogsAppropriateMessage()
        {
            //given
            var target = new DefaultLogger(true, writeLineAction);

            // when
            target.Info("Info message");

            // then
            Assert.That(logOutputLines.Count, Is.EqualTo(1));
            Assert.That(Regex.IsMatch(logOutputLines[0], $"^{LoggerDateTimePattern} INFO  \\[.+?\\] Info message$"), Is.True);
        }

        [Test]
        public void InfoDoesNotLogIfVerboseIsDisabled()
        {
            //given
            var target = new DefaultLogger(false, writeLineAction);

            // when
            target.Info("Info message");

            // then
            Assert.That(logOutputLines, Is.Empty);
        }

        [Test]
        public void DebugLogsAppropriateMessage()
        {
            //given
            var target = new DefaultLogger(true, writeLineAction);

            // when
            target.Debug("Debug message");

            // then
            Assert.That(logOutputLines.Count, Is.EqualTo(1));
            Assert.That(Regex.IsMatch(logOutputLines[0], $"^{LoggerDateTimePattern} DEBUG \\[.+?\\] Debug message$"), Is.True);
        }

        [Test]
        public void DebugDoesNotLogIfVerboseIsDisabled()
        {
            //given
            var target = new DefaultLogger(false, writeLineAction);

            // when
            target.Debug("Debug message");

            // then
            Assert.That(logOutputLines, Is.Empty);
        }

        [Test]
        public void IsErrorEnabledIsTrueIfVerboseIsTrue()
        {
            // then
            Assert.That(new DefaultLogger(true).IsErrorEnabled, Is.True);
        }

        [Test]
        public void IsErrorEnabledIsTrueIfVerboseIsFalse()
        {
            // then
            Assert.That(new DefaultLogger(false).IsErrorEnabled, Is.True);
        }

        [Test]
        public void IsWarnEnabledIsTrueIfVerboseIsTrue()
        {
            // then
            Assert.That(new DefaultLogger(true).IsWarnEnabled, Is.True);
        }

        [Test]
        public void IsWarnEnabledIsTrueIfVerboseIsFalse()
        {
            // then
            Assert.That(new DefaultLogger(false).IsWarnEnabled, Is.True);
        }

        [Test]
        public void IsInfoEnabledIsTrueIfVerboseIsTrue()
        {
            // then
            Assert.That(new DefaultLogger(true).IsInfoEnabled, Is.True);
        }

        [Test]
        public void IsInfoEnabledIsFalseIfVerboseIsFalse()
        {
            // then
            Assert.That(new DefaultLogger(false).IsInfoEnabled, Is.False);
        }

        [Test]
        public void IsDebugEnabledIsTrueIfVerboseIsTrue()
        {
            // then
            Assert.That(new DefaultLogger(true).IsDebugEnabled, Is.True);
        }

        [Test]
        public void IsDebugEnabledIsFalseIfVerboseIsFalse()
        {
            // then
            Assert.That(new DefaultLogger(false).IsDebugEnabled, Is.False);
        }
    }
}
