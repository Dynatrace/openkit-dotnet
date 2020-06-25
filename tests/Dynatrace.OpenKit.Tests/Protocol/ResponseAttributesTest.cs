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
using System.Linq;
using NSubstitute;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Protocol
{
    public class ResponseAttributesTest
    {
        [Test]
        public void BuildWithJsonDefaultsHasNoAttributesSetOnInstance()
        {
            // given
            var target = ResponseAttributes.WithJsonDefaults().Build();

            // when, then
            foreach (var attribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                Assert.That(target.IsAttributeSet(attribute), Is.False);
            }
        }

        [Test]
        public void BuildWithKeyValueDefaultsHasNoAttributeSetOnInstance()
        {
            // given
            var target = ResponseAttributes.WithKeyValueDefaults().Build();

            // when, then
            foreach (var attribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                Assert.That(target.IsAttributeSet(attribute), Is.False);
            }
        }

        [Test]
        public void BuildForwardsJsonDefaultsToInstance()
        {
            // given
            var defaults = ResponseAttributesDefaults.JsonResponse;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.Build();

            // then
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(defaults.MaxBeaconSizeInBytes));
            Assert.That(obtained.MaxSessionDurationInMilliseconds,
                Is.EqualTo(defaults.MaxSessionDurationInMilliseconds));
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(defaults.MaxEventsPerSession));
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(defaults.SessionTimeoutInMilliseconds));
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(defaults.SendIntervalInMilliseconds));
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(defaults.VisitStoreVersion));

            Assert.That(obtained.IsCapture, Is.EqualTo(defaults.IsCapture));
            Assert.That(obtained.IsCaptureCrashes, Is.EqualTo(defaults.IsCaptureCrashes));
            Assert.That(obtained.IsCaptureErrors, Is.EqualTo(defaults.IsCaptureErrors));
            Assert.That(obtained.ApplicationId, Is.EqualTo(defaults.ApplicationId));

            Assert.That(obtained.Multiplicity, Is.EqualTo(defaults.Multiplicity));
            Assert.That(obtained.ServerId, Is.EqualTo(defaults.ServerId));
            Assert.That(obtained.Status, Is.EqualTo(defaults.Status));

            Assert.That(obtained.TimestampInMilliseconds, Is.EqualTo(defaults.TimestampInMilliseconds));
        }

        [Test]
        public void BuildForwardsKeyValueDefaultsToInstance()
        {
            // given
            var defaults = ResponseAttributesDefaults.JsonResponse;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.Build();

            // then
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(defaults.MaxBeaconSizeInBytes));
            Assert.That(obtained.MaxSessionDurationInMilliseconds,
                Is.EqualTo(defaults.MaxSessionDurationInMilliseconds));
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(defaults.MaxEventsPerSession));
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(defaults.SessionTimeoutInMilliseconds));
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(defaults.SendIntervalInMilliseconds));
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(defaults.VisitStoreVersion));

            Assert.That(obtained.IsCapture, Is.EqualTo(defaults.IsCapture));
            Assert.That(obtained.IsCaptureCrashes, Is.EqualTo(defaults.IsCaptureCrashes));
            Assert.That(obtained.IsCaptureErrors, Is.EqualTo(defaults.IsCaptureErrors));
            Assert.That(obtained.ApplicationId, Is.EqualTo(defaults.ApplicationId));

            Assert.That(obtained.Multiplicity, Is.EqualTo(defaults.Multiplicity));
            Assert.That(obtained.ServerId, Is.EqualTo(defaults.ServerId));
            Assert.That(obtained.Status, Is.EqualTo(defaults.Status));

            Assert.That(obtained.TimestampInMilliseconds, Is.EqualTo(defaults.TimestampInMilliseconds));
        }

        [Test]
        public void BuildPropagatesMaxBeaconSizeToInstance()
        {
            // given
            const int beaconSize = 73;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithMaxBeaconSizeInBytes(beaconSize).Build();

            // then
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(beaconSize));
        }

        [Test]
        public void WithMaxBeaconSizeSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.MAX_BEACON_SIZE;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithMaxBeaconSizeInBytes(37).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesMaxSessionDurationToInstance()
        {
            // given
            const int sessionDuration = 73;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithMaxSessionDurationInMilliseconds(sessionDuration).Build();

            // then
            Assert.That(obtained.MaxSessionDurationInMilliseconds, Is.EqualTo(sessionDuration));
        }

        [Test]
        public void WithMaxSessionDurationSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.MAX_SESSION_DURATION;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithMaxSessionDurationInMilliseconds(37).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesMaxEventsPerSessionToInstance()
        {
            // given
            const int eventsPerSession = 73;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithMaxEventsPerSession(eventsPerSession).Build();

            // then
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(eventsPerSession));
        }

        [Test]
        public void WithMaxEventsPerSessionSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.MAX_EVENTS_PER_SESSION;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithMaxEventsPerSession(37).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesSessionTimeoutToInstance()
        {
            // given
            const int sessionTimeout = 73;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithSessionTimeoutInMilliseconds(sessionTimeout).Build();

            // then
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(sessionTimeout));
        }

        [Test]
        public void WithSessionTimeoutSetsAttributeOnInstance()
        {
            // given
            ResponseAttribute attribute = ResponseAttribute.SESSION_IDLE_TIMEOUT;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithSessionTimeoutInMilliseconds(37).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesSendIntervalToInstance()
        {
            // given
            const int sendInterval = 73;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithSendIntervalInMilliseconds(sendInterval).Build();

            // then
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(sendInterval));
        }

        [Test]
        public void WithSendIntervalSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.SEND_INTERVAL;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithSendIntervalInMilliseconds(37).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesVisitStoreVersionToInstance()
        {
            // given
            const int visitStoreVersion = 73;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithVisitStoreVersion(visitStoreVersion).Build();

            // then
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(visitStoreVersion));
        }

        [Test]
        public void WithVisitStoreVersionSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.VISIT_STORE_VERSION;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithVisitStoreVersion(37).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesIsCaptureToInstance()
        {
            // given
            var isCapture = !ResponseAttributesDefaults.JsonResponse.IsCapture;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithCapture(isCapture).Build();

            // then
            Assert.That(obtained.IsCapture, Is.EqualTo(isCapture));
        }

        [Test]
        public void WithCaptureSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.IS_CAPTURE;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithCapture(!ResponseAttributesDefaults.JsonResponse.IsCapture).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesIsCaptureCrashesToInstance()
        {
            // given
            var isCaptureCrashes = !ResponseAttributesDefaults.JsonResponse.IsCaptureCrashes;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithCaptureCrashes(isCaptureCrashes).Build();

            // then
            Assert.That(obtained.IsCaptureCrashes, Is.EqualTo(isCaptureCrashes));
        }

        [Test]
        public void WithCaptureCrashesSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.IS_CAPTURE_CRASHES;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithCaptureCrashes(!ResponseAttributesDefaults.JsonResponse.IsCaptureCrashes).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesIsCaptureErrorsToInstance()
        {
            // given
            var isCaptureErrors = !ResponseAttributesDefaults.JsonResponse.IsCaptureErrors;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithCaptureErrors(isCaptureErrors).Build();

            // then
            Assert.That(obtained.IsCaptureErrors, Is.EqualTo(isCaptureErrors));
        }

        [Test]
        public void WithCaptureErrorsSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.IS_CAPTURE_ERRORS;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithCaptureErrors(!ResponseAttributesDefaults.JsonResponse.IsCaptureErrors).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesApplicationIdToInstance()
        {
            // given
            var applicationId = Guid.NewGuid().ToString();
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithApplicationId(applicationId).Build();

            // then
            Assert.That(obtained.ApplicationId, Is.EqualTo(applicationId));
        }

        [Test]
        public void WithApplicationIdSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.APPLICATION_ID;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithApplicationId(Guid.NewGuid().ToString()).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesMultiplicityToInstance()
        {
            // given
            const int multiplicity = 73;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithMultiplicity(multiplicity).Build();

            // then
            Assert.That(obtained.Multiplicity, Is.EqualTo(multiplicity));
        }

        [Test]
        public void WithMultiplicitySetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.MULTIPLICITY;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithMultiplicity(37).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesServerIdToInstance()
        {
            // given
            const int serverId = 73;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithServerId(serverId).Build();

            // then
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void WithServerIdSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.SERVER_ID;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithServerId(37).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesStatusToInstance()
        {
            // given
            const string status = "status";
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithStatus(status).Build();

            // then
            Assert.That(obtained.Status, Is.EqualTo(status));
        }

        [Test]
        public void WithStatusSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.STATUS;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithStatus("status").Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        [Test]
        public void BuildPropagatesTimestampToInstance()
        {
            // given
            const long timestamp = 73;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithTimestampInMilliseconds(timestamp).Build();

            // then
            Assert.That(obtained.TimestampInMilliseconds, Is.EqualTo(timestamp));
        }

        [Test]
        public void WithTimestampSetsAttributeOnInstance()
        {
            // given
            const ResponseAttribute attribute = ResponseAttribute.TIMESTAMP;
            var target = ResponseAttributes.WithJsonDefaults();

            // when
            var obtained = target.WithTimestampInMilliseconds(37L).Build();

            // then
            Assert.That(obtained.IsAttributeSet(attribute), Is.True);

            foreach (var unsetAttribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                if (attribute == unsetAttribute)
                {
                    continue;
                }

                Assert.That(obtained.IsAttributeSet(unsetAttribute), Is.False);
            }
        }

        #region Merge response

        [Test]
        public void MergingDefaultResponsesReturnsResponseWithoutAnyAttributeSet()
        {
            // given
            var toMerge = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithJsonDefaults().Build();

            // when
            var obtained = target.Merge(toMerge);

            // then
            foreach (var attribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                Assert.That(obtained.IsAttributeSet(attribute), Is.False);
            }
        }

        [Test]
        public void MergeResponseWithAllValuesSetToDefaultResponse()
        {
            // given
            var toMerge = Substitute.For<IResponseAttributes>();
            toMerge.IsAttributeSet(Arg.Any<ResponseAttribute>()).Returns(true);
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(toMerge);

            // then
            foreach (var attribute in Enum.GetValues(typeof(ResponseAttribute)).Cast<ResponseAttribute>())
            {
                Assert.That(obtained.IsAttributeSet(attribute), Is.True);
            }
        }

        [Test]
        public void MergeTakesBeaconSizeFromMergeTargetIfNotSetInSource()
        {
            // given
            const int beaconSize = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithMaxBeaconSizeInBytes(beaconSize).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(beaconSize));
        }

        [Test]
        public void MergeTakesBeaconSizeFromMergeSourceIfSetInSource()
        {
            // given
            const int beaconSize = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithMaxBeaconSizeInBytes(beaconSize).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(beaconSize));
        }

        [Test]
        public void MergeTakesBeaconSizeFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            const int beaconSize = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithMaxBeaconSizeInBytes(beaconSize).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithMaxBeaconSizeInBytes(37).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxBeaconSizeInBytes, Is.EqualTo(beaconSize));
        }

        [Test]
        public void MergeTakesSessionDurationFromMergeTargetIfNotSetInSource()
        {
            // given
            const int sessionDuration = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults()
                .WithMaxSessionDurationInMilliseconds(sessionDuration).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxSessionDurationInMilliseconds, Is.EqualTo(sessionDuration));
        }

        [Test]
        public void MergeTakesSessionDurationFromMergeSourceIfSetInSource()
        {
            // given
            const int sessionDuration = 73;
            var source = ResponseAttributes.WithUndefinedDefaults()
                .WithMaxSessionDurationInMilliseconds(sessionDuration).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxSessionDurationInMilliseconds, Is.EqualTo(sessionDuration));
        }

        [Test]
        public void MergeTakesSessionDurationFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            const int sessionDuration = 73;
            var source = ResponseAttributes.WithUndefinedDefaults()
                .WithMaxSessionDurationInMilliseconds(sessionDuration).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithMaxSessionDurationInMilliseconds(37).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxSessionDurationInMilliseconds, Is.EqualTo(sessionDuration));
        }

        [Test]
        public void MergeTakesEventsPerSessionFromMergeTargetIfNotSetInSource()
        {
            // given
            const int eventsPerSession = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithMaxEventsPerSession(eventsPerSession).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(eventsPerSession));
        }

        [Test]
        public void MergeTakesEventsPerSessionFromMergeSourceIfSetInSource()
        {
            // given
            const int eventsPerSession = 73;
            var source = ResponseAttributes.WithUndefinedDefaults()
                .WithMaxEventsPerSession(eventsPerSession).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(eventsPerSession));
        }

        [Test]
        public void MergeTakesEventsPerSessionFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            const int eventsPerSession = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithMaxEventsPerSession(eventsPerSession).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithMaxEventsPerSession(37).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.MaxEventsPerSession, Is.EqualTo(eventsPerSession));
        }

        [Test]
        public void MergeTakesSessionTimeoutFromMergeTargetIfNotSetInSource()
        {
            // given
            const int sessionTimeout = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithSessionTimeoutInMilliseconds(sessionTimeout)
                .Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(sessionTimeout));
        }

        [Test]
        public void MergeTakesSessionTimeoutFromMergeSourceIfSetInSource()
        {
            // given
            const int sessionTimeout = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithSessionTimeoutInMilliseconds(sessionTimeout)
                .Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(sessionTimeout));
        }

        [Test]
        public void MergeTakesSessionTimeoutFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            const int sessionTimeout = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithSessionTimeoutInMilliseconds(sessionTimeout)
                .Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithSessionTimeoutInMilliseconds(37).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.SessionTimeoutInMilliseconds, Is.EqualTo(sessionTimeout));
        }

        [Test]
        public void MergeTakesSendIntervalFromMergeTargetIfNotSetInSource()
        {
            // given
            const int sendInterval = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithSendIntervalInMilliseconds(sendInterval)
                .Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(sendInterval));
        }

        [Test]
        public void MergeTakesSendIntervalFromMergeSourceIfSetInSource()
        {
            // given
            const int sendInterval = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithSendIntervalInMilliseconds(sendInterval)
                .Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(sendInterval));
        }

        [Test]
        public void MergeTakesSendIntervalFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            const int sendInterval = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithSendIntervalInMilliseconds(sendInterval)
                .Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithSendIntervalInMilliseconds(37).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.SendIntervalInMilliseconds, Is.EqualTo(sendInterval));
        }

        [Test]
        public void MergeTakesVisitStoreVersionFromMergeTargetIfNotSetInSource()
        {
            // given
            const int visitStoreVersion = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithVisitStoreVersion(visitStoreVersion).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(visitStoreVersion));
        }

        [Test]
        public void MergeTakesVisitStoreVersionFromMergeSourceIfSetInSource()
        {
            // given
            const int visitStoreVersion = 73;
            var source = ResponseAttributes.WithUndefinedDefaults()
                .WithVisitStoreVersion(visitStoreVersion).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(visitStoreVersion));
        }

        [Test]
        public void MergeTakesVisitStoreVersionFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            const int visitStoreVersion = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithVisitStoreVersion(visitStoreVersion).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithVisitStoreVersion(37).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.VisitStoreVersion, Is.EqualTo(visitStoreVersion));
        }

        [Test]
        public void MergeTakesCaptureFromMergeTargetIfNotSetInSource()
        {
            // given
            var capture = !ResponseAttributesDefaults.Undefined.IsCapture;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithCapture(capture).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCapture, Is.EqualTo(capture));
        }

        [Test]
        public void MergeTakesCaptureFromMergeSourceIfSetInSource()
        {
            // given
            var capture = !ResponseAttributesDefaults.Undefined.IsCapture;
            var source = ResponseAttributes.WithUndefinedDefaults().WithCapture(capture).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCapture, Is.EqualTo(capture));
        }

        [Test]
        public void MergeTakesCaptureFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            var capture = !ResponseAttributesDefaults.Undefined.IsCapture;
            var source = ResponseAttributes.WithUndefinedDefaults().WithCapture(capture).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithCapture(!capture).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCapture, Is.EqualTo(capture));
        }

        [Test]
        public void MergeTakesCaptureCrashesFromMergeTargetIfNotSetInSource()
        {
            // given
            var captureCrashes = !ResponseAttributesDefaults.Undefined.IsCaptureCrashes;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithCaptureCrashes(captureCrashes).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureCrashes, Is.EqualTo(captureCrashes));
        }

        [Test]
        public void MergeTakesCaptureCrashesFromMergeSourceIfSetInSource()
        {
            // given
            var captureCrashes = !ResponseAttributesDefaults.Undefined.IsCaptureCrashes;
            var source = ResponseAttributes.WithUndefinedDefaults().WithCaptureCrashes(captureCrashes).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureCrashes, Is.EqualTo(captureCrashes));
        }

        [Test]
        public void MergeTakesCaptureCrashesFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            var captureCrashes = !ResponseAttributesDefaults.Undefined.IsCaptureCrashes;
            var source = ResponseAttributes.WithUndefinedDefaults().WithCaptureCrashes(captureCrashes).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithCaptureCrashes(!captureCrashes).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureCrashes, Is.EqualTo(captureCrashes));
        }

        [Test]
        public void MergeTakesCaptureErrorsFromMergeTargetIfNotSetInSource()
        {
            // given
            var captureErrors = !ResponseAttributesDefaults.Undefined.IsCaptureErrors;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithCaptureErrors(captureErrors).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureErrors, Is.EqualTo(captureErrors));
        }

        [Test]
        public void MergeTakesCaptureErrorsFromMergeSourceIfSetInSource()
        {
            // given
            var captureErrors = !ResponseAttributesDefaults.Undefined.IsCaptureErrors;
            var source = ResponseAttributes.WithUndefinedDefaults().WithCaptureErrors(captureErrors).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureErrors, Is.EqualTo(captureErrors));
        }

        [Test]
        public void MergeTakesCaptureErrorsFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            var captureErrors = !ResponseAttributesDefaults.Undefined.IsCaptureErrors;
            var source = ResponseAttributes.WithUndefinedDefaults().WithCaptureErrors(captureErrors).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithCaptureErrors(!captureErrors).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.IsCaptureErrors, Is.EqualTo(captureErrors));
        }

        [Test]
        public void MergeTakesApplicationIdFromMergeTargetIfNotSetInSource()
        {
            // given
            var applicationId = Guid.NewGuid().ToString();
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithApplicationId(applicationId).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ApplicationId, Is.EqualTo(applicationId));
        }

        [Test]
        public void MergeTakesApplicationIdFromMergeSourceIfSetInSource()
        {
            // given
            var applicationId = Guid.NewGuid().ToString();
            var source = ResponseAttributes.WithUndefinedDefaults().WithApplicationId(applicationId).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ApplicationId, Is.EqualTo(applicationId));
        }

        [Test]
        public void MergeTakesApplicationIdFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            var applicationId = Guid.NewGuid().ToString();
            var source = ResponseAttributes.WithUndefinedDefaults().WithApplicationId(applicationId).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithApplicationId(Guid.NewGuid().ToString()).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ApplicationId, Is.EqualTo(applicationId));
        }

        [Test]
        public void MergeTakesMultiplicityFromMergeTargetIfNotSetInSource()
        {
            // given
            const int multiplicity = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithMultiplicity(multiplicity).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.Multiplicity, Is.EqualTo(multiplicity));
        }

        [Test]
        public void MergeTakesMultiplicityFromMergeSourceIfSetInSource()
        {
            // given
            const int multiplicity = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithMultiplicity(multiplicity).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.Multiplicity, Is.EqualTo(multiplicity));
        }

        [Test]
        public void MergeTakesMultiplicityFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            const int multiplicity = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithMultiplicity(multiplicity).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithMultiplicity(37).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.Multiplicity, Is.EqualTo(multiplicity));
        }

        [Test]
        public void MergeTakesServerIdFromMergeTargetIfNotSetInSource()
        {
            // given
            const int serverId = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithServerId(serverId).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void MergeTakesServerIdFromMergeSourceIfSetInSource()
        {
            // given
            const int serverId = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithServerId(serverId).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void MergeTakesServerIdFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            const int serverId = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithServerId(serverId).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithServerId(37).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.ServerId, Is.EqualTo(serverId));
        }

        [Test]
        public void MergeTakesStatusFromMergeTargetIfNotSetInSource()
        {
            // given
            const string status = "status";
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithStatus(status).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.Status, Is.EqualTo(status));
        }

        [Test]
        public void MergeTakesStatusFromMergeSourceIfSetInSource()
        {
            // given
            const string status = "status";
            var source = ResponseAttributes.WithUndefinedDefaults().WithStatus(status).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.Status, Is.EqualTo(status));
        }

        [Test]
        public void MergeTakesStatusFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            const string status = "status";
            var source = ResponseAttributes.WithUndefinedDefaults().WithStatus(status).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithStatus("foobar").Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.Status, Is.EqualTo(status));
        }

        [Test]
        public void MergeTakesTimestampFromMergeTargetIfNotSetInSource()
        {
            // given
            const long timestamp = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithTimestampInMilliseconds(timestamp).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TimestampInMilliseconds, Is.EqualTo(timestamp));
        }

        [Test]
        public void MergeTakesTimestampFromMergeSourceIfSetInSource()
        {
            // given
            const long timestamp = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithTimestampInMilliseconds(timestamp).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TimestampInMilliseconds, Is.EqualTo(timestamp));
        }

        [Test]
        public void MergeTakesTimestampFromMergeSourceIfSetInSourceAndTarget()
        {
            // given
            const long timestamp = 73;
            var source = ResponseAttributes.WithUndefinedDefaults().WithTimestampInMilliseconds(timestamp).Build();
            var target = ResponseAttributes.WithUndefinedDefaults().WithTimestampInMilliseconds(37).Build();

            // when
            var obtained = target.Merge(source);

            // then
            Assert.That(obtained, Is.Not.Null);
            Assert.That(obtained.TimestampInMilliseconds, Is.EqualTo(timestamp));
        }

        #endregion
    }
}