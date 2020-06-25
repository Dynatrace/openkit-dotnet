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

namespace Dynatrace.OpenKit.Providers
{
    /// <summary>
    ///  Class for providing a session ids
    /// </summary>
    public class DefaultSessionIdProvider : ISessionIdProvider
    {
        private int initialIntegerOffset = 0;

        private static readonly object SyncLock = new object();

        public DefaultSessionIdProvider() : this(new DefaultPrnGenerator().NextPositiveInt()) { }

        internal DefaultSessionIdProvider(int initialOffset)
        {
            initialIntegerOffset = initialOffset;
        }

        public int GetNextSessionId()
        {
            lock (SyncLock)
            {
                if (initialIntegerOffset == int.MaxValue)
                {
                    initialIntegerOffset = 0;
                }
                initialIntegerOffset += 1;
                return initialIntegerOffset;
            }
        }
    }
}
