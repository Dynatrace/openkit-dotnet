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

using System;
using Dynatrace.OpenKit.Util.Json;
using Dynatrace.OpenKit.Util.Json.Objects;

namespace Dynatrace.OpenKit.Protocol
{
    public static class JsonResponseParser
    {
        internal const string ResponseKeyAgentConfig = "mobileAgentConfig";
        internal const string ResponseKeyMaxBeaconSizeInKb = "maxBeaconSizeKb";
        internal const string ResponseKeyMaxSessionDurationInMin = "maxSessionDurationMins";
        internal const string ResponseKeyMaxEventsPerSession = "maxEventsPerSession";
        internal const string ResponseKeySessionTimeoutInSec = "sessionTimeoutSec";
        internal const string ResponseKeySendIntervalInSec = "sendIntervalSec";
        internal const string ResponseKeyVisitStoreVersion = "visitStoreVersion";

        internal const string ResponseKeyAppConfig = "appConfig";
        internal const string ResponseKeyCapture = "capture";
        internal const string ResponseKeyReportCrashes = "reportCrashes";
        internal const string ResponseKeyReportErrors = "reportErrors";
        internal const string ResponseKeyApplicationId = "applicationId";

        internal const string ResponseKeyDynamicConfig = "dynamicConfig";
        internal const string ResponseKeyMultiplicity = "multiplicity";
        internal const string ResponseKeyServerId = "serverId";
        internal const string ResponseKeyStatus = "status";

        internal const string ResponseKeyTimestampInMillis = "timestamp";

        public static IResponseAttributes Parse(string jsonResponse)
        {
            var parser = new JsonParser(jsonResponse);

            var parsedValue = parser.Parse();

            var rootObject = (JsonObjectValue) parsedValue;

            var builder = ResponseAttributes.WithJsonDefaults();
            ApplyAgentConfiguration(builder, rootObject);
            ApplyApplicationConfiguration(builder, rootObject);
            ApplyDynamicConfiguration(builder, rootObject);
            ApplyRootAttributes(builder, rootObject);

            return builder.Build();
        }

        #region Agent configuration

        private static void ApplyAgentConfiguration(ResponseAttributes.Builder builder, JsonObjectValue rootObject)
        {
            if (!(rootObject[ResponseKeyAgentConfig] is JsonObjectValue agentConfigObject))
            {
                return;
            }

            ApplyBeaconSizeInKb(builder, agentConfigObject);
            ApplyMaxSessionDurationInMin(builder, agentConfigObject);
            ApplyMaxEventsPerSession(builder, agentConfigObject);
            ApplySessionTimeoutInSec(builder, agentConfigObject);
            ApplySendIntervalInSec(builder, agentConfigObject);
            ApplyVisitStoreVersion(builder, agentConfigObject);
        }

        private static void ApplyBeaconSizeInKb(ResponseAttributes.Builder builder, JsonObjectValue agentConfigObject)
        {
            if (!(agentConfigObject[ResponseKeyMaxBeaconSizeInKb] is JsonNumberValue numberValue))
            {
                return;
            }

            var beaconSizeInKb = numberValue.IntValue;
            builder.WithMaxBeaconSizeInBytes(beaconSizeInKb * 1024);
        }

        private static void ApplyMaxSessionDurationInMin(ResponseAttributes.Builder builder,
            JsonObjectValue agentConfigObject)
        {
            if (!(agentConfigObject[ResponseKeyMaxSessionDurationInMin] is JsonNumberValue numberValue))
            {
                return;
            }

            var sessionDurationMillis = (int) TimeSpan.FromMinutes(numberValue.IntValue).TotalMilliseconds;
            builder.WithMaxSessionDurationInMilliseconds(sessionDurationMillis);
        }

        private static void ApplyMaxEventsPerSession(ResponseAttributes.Builder builder,
            JsonObjectValue agentConfigObject)
        {
            if (!(agentConfigObject[ResponseKeyMaxEventsPerSession] is JsonNumberValue numberValue))
            {
                return;
            }

            var maxEvents = numberValue.IntValue;
            builder.WithMaxEventsPerSession(maxEvents);
        }

        private static void ApplySessionTimeoutInSec(ResponseAttributes.Builder builder,
            JsonObjectValue agentConfigObject)
        {
            if (!(agentConfigObject[ResponseKeySessionTimeoutInSec] is JsonNumberValue numberValue))
            {
                return;
            }

            var timeoutInMillis = (int) TimeSpan.FromSeconds(numberValue.IntValue).TotalMilliseconds;
            builder.WithSessionTimeoutInMilliseconds(timeoutInMillis);
        }

        private static void ApplySendIntervalInSec(ResponseAttributes.Builder builder,
            JsonObjectValue agentConfigObject)
        {
            if (!(agentConfigObject[ResponseKeySendIntervalInSec] is JsonNumberValue numberValue))
            {
                return;
            }

            var intervalInMillis = (int) TimeSpan.FromSeconds(numberValue.IntValue).TotalMilliseconds;
            builder.WithSendIntervalInMilliseconds(intervalInMillis);
        }

        private static void ApplyVisitStoreVersion(ResponseAttributes.Builder builder,
            JsonObjectValue agentConfigObject)
        {
            if (!(agentConfigObject[ResponseKeyVisitStoreVersion] is JsonNumberValue numberValue))
            {
                return;
            }

            var visitStoreVersion = numberValue.IntValue;
            builder.WithVisitStoreVersion(visitStoreVersion);
        }

        #endregion

        #region Application configuration

        private static void ApplyApplicationConfiguration(ResponseAttributes.Builder builder,
            JsonObjectValue rootObject)
        {
            if (!(rootObject[ResponseKeyAppConfig] is JsonObjectValue appConfigObject))
            {
                return;
            }

            ApplyCapture(builder, appConfigObject);
            ApplyReportCrashes(builder, appConfigObject);
            ApplyReportErrors(builder, appConfigObject);
            ApplyApplicationId(builder, appConfigObject);
        }

        private static void ApplyCapture(ResponseAttributes.Builder builder, JsonObjectValue appConfigObject)
        {
            if (!(appConfigObject[ResponseKeyCapture] is JsonNumberValue numberValue))
            {
                return;
            }

            var capture = numberValue.IntValue;
            builder.WithCapture(capture == 1);
        }

        private static void ApplyReportCrashes(ResponseAttributes.Builder builder, JsonObjectValue appConfigObject)
        {
            if (!(appConfigObject[ResponseKeyReportCrashes] is JsonNumberValue numberValue))
            {
                return;
            }

            var reportCrashes = numberValue.IntValue;
            builder.WithCaptureCrashes(reportCrashes != 0);
        }

        private static void ApplyReportErrors(ResponseAttributes.Builder builder, JsonObjectValue appConfigObject)
        {
            if (!(appConfigObject[ResponseKeyReportErrors] is JsonNumberValue numberValue))
            {
                return;
            }

            var reportErrors = numberValue.IntValue;
            builder.WithCaptureErrors(reportErrors != 0);
        }

        private static void ApplyApplicationId(ResponseAttributes.Builder builder, JsonObjectValue appConfigObject)
        {
            if (!(appConfigObject[ResponseKeyApplicationId] is JsonStringValue stringValue))
            {
                return;
            }
            
            builder.WithApplicationId(stringValue.Value);
        }

        #endregion

        #region Dynamic configuration

        private static void ApplyDynamicConfiguration(ResponseAttributes.Builder builder, JsonObjectValue rootObject)
        {
            if (!(rootObject[ResponseKeyDynamicConfig] is JsonObjectValue dynConfigObject))
            {
                return;
            }

            ApplyMultiplicity(builder, dynConfigObject);
            ApplyServerId(builder, dynConfigObject);
            ApplyStatus(builder, dynConfigObject);
        }

        private static void ApplyMultiplicity(ResponseAttributes.Builder builder, JsonObjectValue dynConfigObject)
        {
            if (!(dynConfigObject[ResponseKeyMultiplicity] is JsonNumberValue numberValue))
            {
                return;
            }

            var multiplicity = numberValue.IntValue;
            builder.WithMultiplicity(multiplicity);
        }

        private static void ApplyServerId(ResponseAttributes.Builder builder, JsonObjectValue dynConfigObject)
        {
            if (!(dynConfigObject[ResponseKeyServerId] is JsonNumberValue numberValue))
            {
                return;
            }

            var serverId = numberValue.IntValue;
            builder.WithServerId(serverId);
        }

        private static void ApplyStatus(ResponseAttributes.Builder builder, JsonObjectValue dynConfigObject)
        {
            if (!(dynConfigObject[ResponseKeyStatus] is JsonStringValue stringValue))
            {
                return;
            }
            
            builder.WithStatus(stringValue.Value);
        }

        #endregion

        #region Root object configuration

        private static void ApplyRootAttributes(ResponseAttributes.Builder builder, JsonObjectValue rootObject)
        {
            if (!(rootObject[ResponseKeyTimestampInMillis] is JsonNumberValue numberValue))
            {
                return;
            }

            var timestamp = numberValue.LongValue;
            builder.WithTimestampInMilliseconds(timestamp);
        }

        #endregion
    }
}