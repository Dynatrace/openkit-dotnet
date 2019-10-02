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

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class PrivacyConfigurationTest
    {
        [Test]
        public void DataCollectionLevelReturnsLevelPassedInConstructor()
        {
            // when, then
            Assert.That(new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF)
                .DataCollectionLevel, Is.EqualTo(DataCollectionLevel.OFF));
            Assert.That(new PrivacyConfiguration(DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF)
                .DataCollectionLevel, Is.EqualTo(DataCollectionLevel.PERFORMANCE));
            Assert.That(new PrivacyConfiguration(DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF)
                .DataCollectionLevel, Is.EqualTo(DataCollectionLevel.USER_BEHAVIOR));
        }

        [Test]
        public void CrashReportingLevelReturnsLevelPassedInConstructor()
        {
            // when, then
            Assert.That(new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF)
                .CrashReportingLevel, Is.EqualTo(CrashReportingLevel.OFF));
            Assert.That(new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OPT_IN_CRASHES)
                .CrashReportingLevel, Is.EqualTo(CrashReportingLevel.OPT_IN_CRASHES));
            Assert.That(new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OPT_OUT_CRASHES)
                .CrashReportingLevel, Is.EqualTo(CrashReportingLevel.OPT_OUT_CRASHES));
        }

        #region session number reporting

        [Test]
        public void SessionNumberReportingIsAllowedIfDataCollectionLevelIsEqualToUserBehavior()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsSessionNumberReportingAllowed, Is.True);
        }

        [Test]
        public void SessionNumberReportingIsNotAllowedIfDataCollectionLevelIsEqualToPerformance()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsSessionNumberReportingAllowed, Is.False);
        }

        [Test]
        public void SessionNumberReportingIsNotAllowedIfDataCollectionLevelIsEqualToOff()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsSessionNumberReportingAllowed, Is.False);
        }

        #endregion

        #region device ID sending

        [Test]
        public void DeviceIdSendingIsAllowedIfDataCollectionLevelIsEqualToUserBehavior()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsDeviceIdSendingAllowed, Is.True);
        }

        [Test]
        public void DeviceIdSendingIsNotAllowedIfDataCollectionLevelIsEqualToPerformance()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsDeviceIdSendingAllowed, Is.False);
        }

        [Test]
        public void DeviceIdSendingIsNotAllowedIfDataCollectionLevelIsEqualToOff()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsDeviceIdSendingAllowed, Is.False);
        }

        #endregion

        #region web request tracing

        [Test]
        public void WebRequestTracingIsAllowedIfDataCollectionLevelIsEqualToUserBehavior()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsWebRequestTracingAllowed, Is.True);
        }

        [Test]
        public void WebRequestTracingIsAllowedIfDataCollectionLevelIsEqualToPerformance()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsWebRequestTracingAllowed, Is.True);
        }

        [Test]
        public void WebRequestTracingIsNotAllowedIfDataCollectionLevelIsEqualToOff()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsWebRequestTracingAllowed, Is.False);
        }

        #endregion

        #region session reporting

        [Test]
        public void SessionReportingIsAllowedIfDataCollectionLevelIsEqualToUserBehavior()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsSessionReportingAllowed, Is.True);
        }

        [Test]
        public void SessionReportingIsAllowedIfDataCollectionLevelIsEqualToPerformance()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsSessionReportingAllowed, Is.True);
        }

        [Test]
        public void SessionReportingIsNotAllowedIfDataCollectionLevelIsEqualToOff()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsSessionReportingAllowed, Is.False);
        }

        #endregion

        #region action reporting

        [Test]
        public void ActionReportingIsAllowedIfDataCollectionLevelIsEqualToUserBehavior()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsActionReportingAllowed, Is.True);
        }

        [Test]
        public void ActionReportingIsAllowedIfDataCollectionLevelIsEqualToPerformance()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsActionReportingAllowed, Is.True);
        }

        [Test]
        public void ActionReportingIsNotAllowedIfDataCollectionLevelIsEqualToOff()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsActionReportingAllowed, Is.False);
        }

        #endregion

        #region value reporting

        [Test]
        public void ValueReportingIsAllowedIfDataCollectionLevelIsEqualToUserBehavior()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsValueReportingAllowed, Is.True);
        }

        [Test]
        public void ValueReportingIsNotAllowedIfDataCollectionLevelIsEqualToPerformance()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsValueReportingAllowed, Is.False);
        }

        [Test]
        public void ValueReportingIsNotAllowedIfDataCollectionLevelIsEqualToOff()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsValueReportingAllowed, Is.False);
        }

        #endregion

        #region event reporting

        [Test]
        public void EventReportingIsAllowedIfDataCollectionLevelIsEqualToUserBehavior()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsEventReportingAllowed, Is.True);
        }

        [Test]
        public void EventReportingIsNotAllowedIfDataCollectionLevelIsEqualToPerformance()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsEventReportingAllowed, Is.False);
        }

        [Test]
        public void EventReportingIsNotAllowedIfDataCollectionLevelIsEqualToOff()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsEventReportingAllowed, Is.False);
        }

        #endregion

        #region error reporting

        [Test]
        public void ErrorReportingIsAllowedIfDataCollectionLevelIsEqualToUserBehavior()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsErrorReportingAllowed, Is.True);
        }

        [Test]
        public void ErrorReportingIsAllowedIfDataCollectionLevelIsEqualToPerformance()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsErrorReportingAllowed, Is.True);
        }

        [Test]
        public void ErrorReportingIsNotAllowedIfDataCollectionLevelIsEqualToOff()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsErrorReportingAllowed, Is.False);
        }

        #endregion

        #region crash reporting

        [Test]
        public void CrashReportingIsAllowedIfCrashReportingLevelIsEqualToOptInCrashes()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OPT_IN_CRASHES);

            // when, then
            Assert.That(target.IsCrashReportingAllowed, Is.True);
        }

        [Test]
        public void CrashReportingIsNotAllowedIfCrashReportingLevelIsEqualToOptOutCrashes()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OPT_OUT_CRASHES);

            // when, then
            Assert.That(target.IsCrashReportingAllowed, Is.False);
        }

        [Test]
        public void CrashReportingIsNotAllowedIfCrashReportingLevelIsEqualToOff()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsCrashReportingAllowed, Is.False);
        }

        #endregion

        #region user identification

        [Test]
        public void UserIdentificationIsAllowedIfDataCollectionLevelIsEqualToUserBehavior()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.USER_BEHAVIOR, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsUserIdentificationIsAllowed, Is.True);
        }

        [Test]
        public void UserIdentificationIsNotAllowedIfDataCollectionLevelIsEqualToPerformance()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.PERFORMANCE, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsUserIdentificationIsAllowed, Is.False);
        }

        [Test]
        public void UserIdentificationIsNotAllowedIfDataCollectionLevelIsEqualToOff()
        {
            // given
            var target = new PrivacyConfiguration(DataCollectionLevel.OFF, CrashReportingLevel.OFF);

            // when, then
            Assert.That(target.IsUserIdentificationIsAllowed, Is.False);
        }

        #endregion
    }
}