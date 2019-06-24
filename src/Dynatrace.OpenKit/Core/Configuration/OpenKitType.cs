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

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class OpenKitType
    {

        public static readonly OpenKitType APPMON = new OpenKitType("dynaTraceMonitor", 1, "APPMON");         // AppMon: default monitor URL name contains "dynaTraceMonitor" and default Server ID is 1
        public static readonly OpenKitType DYNATRACE = new OpenKitType("mbeacon", -1, "DYNATRACE");              // Dynatrace: default monitor URL name contains "mbeacon" and default Server ID is -1

        OpenKitType(string defaultMonitorName, int defaultServerID, string typeName)
        {
            DefaultMonitorName = defaultMonitorName;
            DefaultServerID = defaultServerID;
            TypeName = typeName;
        }

        public string DefaultMonitorName { get; }

        public int DefaultServerID { get; }

        public string TypeName { get; }

        public override string ToString()
        {
            return TypeName;
        }

    }
}
