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

using Dynatrace.OpenKit.API;
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
            var target = new DefaultLogger(LogLevel.DEBUG, writeLineAction);

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
            var target = new DefaultLogger(LogLevel.DEBUG, writeLineAction);

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
            var target = new DefaultLogger(LogLevel.DEBUG, writeLineAction);

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
            var target = new DefaultLogger(LogLevel.DEBUG, writeLineAction);

            // when
            target.Info("Info message");

            // then
            Assert.That(logOutputLines.Count, Is.EqualTo(1));
            Assert.That(Regex.IsMatch(logOutputLines[0], $"^{LoggerDateTimePattern} INFO  \\[.+?\\] Info message$"), Is.True);
        }

        [Test]
        public void InfoDoesNotLogIfLogLevelInfoIsDisabled()
        {
            //given
            var target = new DefaultLogger(LogLevel.WARN, writeLineAction);

            // when
            target.Info("Info message");

            // then
            Assert.That(logOutputLines, Is.Empty);
        }

        [Test]
        public void DebugLogsAppropriateMessage()
        {
            //given
            var target = new DefaultLogger(LogLevel.DEBUG, writeLineAction);

            // when
            target.Debug("Debug message");

            // then
            Assert.That(logOutputLines.Count, Is.EqualTo(1));
            Assert.That(Regex.IsMatch(logOutputLines[0], $"^{LoggerDateTimePattern} DEBUG \\[.+?\\] Debug message$"), Is.True);
        }

        [Test]
        public void InfoDoesNotLogIfLogLevelDebugIsDisabled()
        {
            //given
            var target = new DefaultLogger(LogLevel.INFO, writeLineAction);

            // when
            target.Debug("Debug message");

            // then
            Assert.That(logOutputLines, Is.Empty);
        }

        [Test]
        public void IsErrorEnabledIsTrueIfLevelIsLessThanOrEqualToLevelError()
        {
            // when <= ERROR, then
            Assert.That(new DefaultLogger(LogLevel.ERROR).IsErrorEnabled, Is.True);
            Assert.That(new DefaultLogger(LogLevel.ERROR - 1).IsErrorEnabled, Is.True);
        }

        [Test]
        public void IsErrorEnabledIsFalseIfLevelIsGreaterThanLevelError()
        {
            // when > ERROR, then
            Assert.That(new DefaultLogger(LogLevel.ERROR + 1).IsErrorEnabled, Is.False);
        }

        [Test]
        public void IsWarnEnabledIsTrueIfLevelIsLessThanOrEqualToLevelWarn()
        {
            // when <= WARN, then
            Assert.That(new DefaultLogger(LogLevel.WARN).IsWarnEnabled, Is.True);
            Assert.That(new DefaultLogger(LogLevel.WARN - 1).IsWarnEnabled, Is.True);
        }

        [Test]
        public void IsWarnEnabledIsFalseIfLevelIsGreaterThanLevelWarn()
        {
            // when > WARN, then
            Assert.That(new DefaultLogger(LogLevel.WARN + 1).IsWarnEnabled, Is.False);
        }

        [Test]
        public void IsInfoEnabledIsTrueIfLevelIsLessThanOrEqualToLevelInfo()
        {
            // when <= INFO, then
            Assert.That(new DefaultLogger(LogLevel.INFO).IsInfoEnabled, Is.True);
            Assert.That(new DefaultLogger(LogLevel.INFO - 1).IsInfoEnabled, Is.True);
        }

        [Test]
        public void IsInfoEnabledIsFalseIfLevelIsGreaterThanLevelInfo()
        {
            // when > INFO, then
            Assert.That(new DefaultLogger(LogLevel.INFO + 1).IsInfoEnabled, Is.False);
        }

        [Test]
        public void IsDebugEnabledIsTrueIfLevelIsLessThanOrEqualToLevelDebug()
        {
            // when <= DEBUG, then
            Assert.That(new DefaultLogger(LogLevel.DEBUG).IsDebugEnabled, Is.True);
            Assert.That(new DefaultLogger(LogLevel.DEBUG - 1).IsDebugEnabled, Is.True);
        }

        [Test]
        public void IsDebugEnabledIsFalseIfLevelIsGreaterThanLevelDebug()
        {
            // when > DEBUG, then
            Assert.That(new DefaultLogger(LogLevel.DEBUG + 1).IsDebugEnabled, Is.False);
        }
    }
}
