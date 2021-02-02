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

using System.Globalization;

namespace Dynatrace.OpenKit.Util
{
    /// <summary>
    /// Extension class providing culture invariant to string methods.
    /// </summary>
    public static class ToStringExtensions
    {
        #region integer numbers

        /// <summary>
        /// Convert <code>short</code> value to <code>string</code>
        /// with <see cref="CultureInfo.InvariantCulture"/>
        /// </summary>
        public static string ToInvariantString(this short value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert <code>ushort</code> value to <code>string</code>
        /// with <see cref="CultureInfo.InvariantCulture"/>
        /// </summary>
        public static string ToInvariantString(this ushort value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert <code>int</code> value to <code>string</code>
        /// with <see cref="CultureInfo.InvariantCulture"/>
        /// </summary>
        public static string ToInvariantString(this int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert <code>uint</code> value to <code>string</code>
        /// with <see cref="CultureInfo.InvariantCulture"/>
        /// </summary>
        public static string ToInvariantString(this uint value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert <code>long</code> value to <code>string</code>
        /// with <see cref="CultureInfo.InvariantCulture"/>
        /// </summary>
        public static string ToInvariantString(this long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert <code>ulong</code> value to <code>string</code>
        /// with <see cref="CultureInfo.InvariantCulture"/>
        /// </summary>
        public static string ToInvariantString(this ulong value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region floating point numbers

        /// <summary>
        /// Convert <code>float</code> value to <code>string</code>
        /// with <see cref="CultureInfo.InvariantCulture"/>
        /// </summary>
        public static string ToInvariantString(this float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert <code>double</code> value to <code>string</code>
        /// with <see cref="CultureInfo.InvariantCulture"/>
        /// </summary>
        public static string ToInvariantString(this double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        #endregion
    }
}
