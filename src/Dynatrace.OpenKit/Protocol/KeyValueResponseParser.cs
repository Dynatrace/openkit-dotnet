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
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Protocol
{
    public static class KeyValueResponseParser
    {
        private static readonly char[] PartsSeparator = {'&'};

        internal const string ResponseKeyMaxBeaconSizeInKb = "bl";
        internal const string ResponseKeySendIntervalInSec = "si";

        internal const string ResponseKeyCapture = "cp";
        internal const string ResponseKeyReportCrashes = "cr";
        internal const string ResponseKeyReportErrors = "er";
        internal const string ResponseKeyTrafficControlPercentage = "tc";

        internal const string ResponseKeyServerId = "id";
        internal const string ResponseKeyMultiplicity = "mp";

        public static IResponseAttributes Parse(string keyValuePairResponse)
        {
            var keyValuePairs = ParseKeyValuePairs(keyValuePairResponse);

            var builder = ResponseAttributes.WithKeyValueDefaults();

            ApplyBeaconSizeInKb(builder, keyValuePairs);
            ApplySendIntervalInSec(builder, keyValuePairs);
            ApplyCapture(builder, keyValuePairs);
            ApplyReportCrashes(builder, keyValuePairs);
            ApplyReportErrors(builder, keyValuePairs);
            ApplyTrafficControlPercentage(builder, keyValuePairs);
            ApplyServerId(builder, keyValuePairs);
            ApplyMultiplicity(builder, keyValuePairs);

            return builder.Build();
        }

        #region Key/Value parsing

        private static Dictionary<string, string> ParseKeyValuePairs(string response)
        {
            var resultDictionary = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(response))
            {
                return resultDictionary;
            }

            foreach (var parts in response.Split(PartsSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                var tokens = parts.Split('=');
                if (tokens.Length != 2)
                {
                    throw new ArgumentException("Invalid response; even number of tokens expected.");
                }

                var key = tokens[0];
                var value = tokens[1];

                resultDictionary[key] = value;
            }

            return resultDictionary;
        }

        #endregion

        #region extract attributes

        private static void ApplyBeaconSizeInKb(ResponseAttributes.Builder builder,
            Dictionary<string, string> keyValuePairs)
        {
            if (!keyValuePairs.TryGetValue(ResponseKeyMaxBeaconSizeInKb, out var beaconSizeKbString))
            {
                return;
            }

            var beaconSizeInKb = int.Parse(beaconSizeKbString);
            builder.WithMaxBeaconSizeInBytes(beaconSizeInKb * 1024);
        }

        private static void ApplySendIntervalInSec(ResponseAttributes.Builder builder,
            Dictionary<string, string> keyValuePairs)
        {
            if (!keyValuePairs.TryGetValue(ResponseKeySendIntervalInSec, out var sendIntervalSecString))
            {
                return;
            }

            var sendIntervalInSec = int.Parse(sendIntervalSecString);
            builder.WithSendIntervalInMilliseconds((int) TimeSpan.FromSeconds(sendIntervalInSec).TotalMilliseconds);
        }

        private static void ApplyCapture(ResponseAttributes.Builder builder,
            Dictionary<string, string> keyValuePairs)
        {
            if (!keyValuePairs.TryGetValue(ResponseKeyCapture, out var captureString))
            {
                return;
            }

            var capture = int.Parse(captureString);
            builder.WithCapture(capture == 1);
        }

        private static void ApplyReportCrashes(ResponseAttributes.Builder builder,
            Dictionary<string, string> keyValuePairs)
        {
            if (!keyValuePairs.TryGetValue(ResponseKeyReportCrashes, out var reportCrashesString))
            {
                return;
            }

            var reportCrashes = int.Parse(reportCrashesString);
            builder.WithCaptureCrashes(reportCrashes != 0);
        }

        private static void ApplyReportErrors(ResponseAttributes.Builder builder,
            Dictionary<string, string> keyValuePairs)
        {
            if (!keyValuePairs.TryGetValue(ResponseKeyReportErrors, out var reportErrorsString))
            {
                return;
            }

            var reportErrors = int.Parse(reportErrorsString);
            builder.WithCaptureErrors(reportErrors != 0);
        }

        private static void ApplyTrafficControlPercentage(ResponseAttributes.Builder builder,
            Dictionary<string, string> keyValuePairs)
        {
            if (!keyValuePairs.TryGetValue(ResponseKeyTrafficControlPercentage, out var tcValue))
            {
                return;
            }

            var trafficControlPercentage = int.Parse(tcValue);
            builder.WithTrafficControlPercentage(trafficControlPercentage);
        }

        private static void ApplyServerId(ResponseAttributes.Builder builder,
            Dictionary<string, string> keyValuePairs)
        {
            if (!keyValuePairs.TryGetValue(ResponseKeyServerId, out var serverIdString))
            {
                return;
            }

            var serverId = int.Parse(serverIdString);
            builder.WithServerId(serverId);
        }

        private static void ApplyMultiplicity(ResponseAttributes.Builder builder,
            Dictionary<string, string> keyValuePairs)
        {
            if (!keyValuePairs.TryGetValue(ResponseKeyMultiplicity, out var multiplicityString))
            {
                return;
            }

            var multiplicity = int.Parse(multiplicityString);
            builder.WithMultiplicity(multiplicity);
        }

        #endregion
    }
}