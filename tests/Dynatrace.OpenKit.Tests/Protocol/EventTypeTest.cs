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

using Dynatrace.OpenKit.Util;
using NUnit.Framework;

namespace Dynatrace.OpenKit.Protocol
{
    public class EventTypeTest
    {
        [TestCase(1,    EventType.ACTION)]
        [TestCase(10,   EventType.NAMED_EVENT)]
        [TestCase(11,   EventType.VALUE_STRING)]
        [TestCase(12,   EventType.VALUE_INT)]
        [TestCase(13,   EventType.VALUE_DOUBLE)]
        [TestCase(18,   EventType.SESSION_START)]
        [TestCase(19,   EventType.SESSION_END)]
        [TestCase(30,   EventType.WEB_REQUEST)]
        [TestCase(40,   EventType.ERROR)]
        [TestCase(50,   EventType.CRASH)]
        [TestCase(60,   EventType.IDENTIFY_USER)]
        public void ToInt(int expected, EventType input)
        {
            Assert.That(input.ToInt(), Is.EqualTo(expected));
        }
    }
}