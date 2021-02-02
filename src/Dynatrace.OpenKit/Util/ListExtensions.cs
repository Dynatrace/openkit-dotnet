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

#if NETSTANDARD1_1

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dynatrace.OpenKit.Util
{

    /// <summary>
    /// Utility class providing some extension methods which are not available in certain .NET flavors.
    /// </summary>
    public static class ListExtensions
    {
        public static ReadOnlyCollection<T> AsReadOnly<T>(this List<T> source)
        {
            return new ReadOnlyCollection<T>(source);
        }

        public static void ForEach<T>(this List<T> source, Action<T> action)
        {
            foreach (T element in source)
            {
                action(element);
            }
        }
    }
}
#endif
