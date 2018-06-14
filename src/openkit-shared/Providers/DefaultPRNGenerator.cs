//
// Copyright 2018 Dynatrace LLC
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

namespace Dynatrace.OpenKit.Providers
{
    /// <summary>
    /// Interface providing random numbers
    /// </summary>
    class DefaultPRNGenerator : IPRNGenerator
    {
        private readonly System.Random randomGenerator;

        public int NextInt(int lowerBoundary, int upperBoundary)
        {
            return randomGenerator.Next(lowerBoundary, upperBoundary);
        }

        public long NextLong(long lowerBoundary, long upperBoundary)
        {
            byte[] buf = new byte[8];
            new System.Random().NextBytes(buf);
            var longValue = System.BitConverter.ToInt64(buf, 0);
            return Math.Abs(longValue % (upperBoundary - lowerBoundary)) + lowerBoundary;
        }
    }
}
