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

namespace Dynatrace.OpenKit.Util
{
    public static class StringUtil
    {
        /// <summary>
        /// Generates a 64 bit hash from the given string.
        /// </summary>
        /// <returns>the 64 bit hash for the given string (<code>0</code> in case the given string is <code>null</code> or empty</returns>
        /// <param name="stringValue">the value to be hashed</param>
        public static long To64BitHash(string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                return 0;
            }

            long hash1 = 5381;
            long hash2 = hash1;

            int index = 0;
            while (index < stringValue.Length)
            {
                hash1 = ((hash1 << 5) + hash1) ^ stringValue[index];

                if (index + 1 >= stringValue.Length)
                {
                    break;
                }

                hash2 = ((hash2 << 5) + hash2) ^ stringValue[index + 1];

                index += 2;
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}