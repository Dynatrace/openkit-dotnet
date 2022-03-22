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

namespace Dynatrace.OpenKit.Protocol
{

    /// <summary>
    ///  Event types used in the beacon protocol.
    /// </summary>
    public enum EventType
    {
        ACTION = 1,                     // Action
        VALUE_STRING = 11,              // captured string
        VALUE_INT = 12,                 // captured int
        VALUE_DOUBLE = 13,              // captured double
        NAMED_EVENT = 10,               // named event
        SESSION_START = 18,             // session start
        SESSION_END = 19,               // session end
        WEB_REQUEST = 30,               // tagged web request
        ERROR = 40,                     // error
        EXCEPTION = 42,                 // exception
        CRASH = 50,                     // crash
        IDENTIFY_USER = 60,             // identify user
        EVENT = 98                      // event api
    }

}
