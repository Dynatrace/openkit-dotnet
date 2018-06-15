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

namespace Dynatrace.OpenKit.Providers
{
    /// <summary>
    /// Interface providing random numbers
    /// </summary>
    public interface IPRNGenerator
    {
        /// <summary>
        /// Provide a random int between 0 (inclusive) and upperBoundary (exclusive)
        /// </summary>
        /// <param name="upperBoundary">value of upper boundary</param>
        /// <returns>random int value between 0 and upper boundary</returns>
        int NextInt(int upperBoundary);

        /// <summary>
        /// Provide a random long between 0 (inclusive) and upperBoundary (exclusive)
        /// </summary
        /// <param name="upperBoundary">value of upper boundary</param>
        /// <returns>random long value between 0 and upper Boundary</returns>
        long NextLong(long upperBoundary);
    }
}
