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

namespace Dynatrace.OpenKit.Protocol
{
    public static class ResponseAttributesDefaults
    {
        public static readonly IResponseAttributes JsonResponse = new JsonResponseDefaults();
        public static readonly IResponseAttributes KeyValueResponse = new KeyValueResponseDefaults();
        public static readonly IResponseAttributes Undefined = new UndefinedDefaults();

        private abstract class AbstractResponseDefaults : IResponseAttributes
        {
            public abstract int MaxBeaconSizeInBytes { get; }

            public abstract int MaxSessionDurationInMilliseconds { get; }

            public abstract int MaxEventsPerSession { get; }

            public abstract int SessionTimeoutInMilliseconds { get; }

            public virtual int SendIntervalInMilliseconds { get; } = (int) TimeSpan.FromSeconds(120).TotalMilliseconds;

            public int VisitStoreVersion => 1;

            public bool IsCapture => true;

            public bool IsCaptureCrashes => true;

            public bool IsCaptureErrors => true;

            public string ApplicationId => null;

            public int Multiplicity => 1;

            public virtual int ServerId => 1;

            public long TimestampInMilliseconds => 0;

            public bool IsAttributeSet(ResponseAttribute attribute)
            {
                return false;
            }

            public IResponseAttributes Merge(IResponseAttributes responseAttributes)
            {
                return responseAttributes;
            }
        }

        private class JsonResponseDefaults : AbstractResponseDefaults
        {
            public override int MaxBeaconSizeInBytes { get; } = 150 * 1024; // 150 kB

            public override int MaxSessionDurationInMilliseconds { get; } =
                (int) TimeSpan.FromMinutes(360).TotalMilliseconds; // 360 minutes

            public override int MaxEventsPerSession { get; } = 200;

            public override int SessionTimeoutInMilliseconds { get; } =
                (int) TimeSpan.FromSeconds(600).TotalMilliseconds; // 600 seconds
        }

        private class KeyValueResponseDefaults : AbstractResponseDefaults
        {
            public override int MaxBeaconSizeInBytes { get; } = 30 * 1024;

            public override int MaxSessionDurationInMilliseconds { get; } = -1;

            public override int MaxEventsPerSession { get; } = -1;

            public override int SessionTimeoutInMilliseconds { get; } = -1;
        }

        private class UndefinedDefaults : AbstractResponseDefaults
        {
            public override int MaxBeaconSizeInBytes { get; } = 30 * 1024; // 30 kB

            public override int MaxSessionDurationInMilliseconds { get; } = -1;

            public override int MaxEventsPerSession { get; } = -1;

            public override int SessionTimeoutInMilliseconds { get; } = -1;

            public override int ServerId { get; } = -1;
        }
    }
}