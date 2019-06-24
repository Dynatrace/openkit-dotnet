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
using System.Collections;
using System.Linq;
using System.Text;

namespace Dynatrace.OpenKit.Core.Util
{
    /// <summary>
    /// Utility class for percent-encoding (also known as URL encoding) strings.
    /// </summary>
    /// <remarks>
    /// This class shall be used to percent escape (URL encode) strings.
    /// 
    /// Reserved characters are based on RFC 3986 (<seealso cref="https://tools.ietf.org/html/rfc3986#section-2.3"/>)
    /// </remarks>
    public static class PercentEncoder
    {
        private const int UNRESERVED_CHARACTERS_BITS = 128;
        private static readonly BitArray UNRESERVED_CHARACTERS = new BitArray(UNRESERVED_CHARACTERS_BITS);

        /// <summary>
        /// Static constructor used to initialize <code>UNRESERVED_CHARACTERS_BITS</code>
        /// </summary>
        static PercentEncoder()
        {
            // 26 lower case letters, starting with 'a'
            Enumerable.Range('a', 26).ForEach(c => UNRESERVED_CHARACTERS[c] = true);
            // 26 upper case letters, starting with 'A'
            Enumerable.Range('A', 26).ForEach(c => UNRESERVED_CHARACTERS[c] = true);
            // 10 digits, starting with '0'
            Enumerable.Range('0', 10).ForEach(c => UNRESERVED_CHARACTERS[c] = true);
            // special characters based on RFC
            UNRESERVED_CHARACTERS['-'] = true;
            UNRESERVED_CHARACTERS['.'] = true;
            UNRESERVED_CHARACTERS['_'] = true;
            UNRESERVED_CHARACTERS['~'] = true;
        }

        /// <summary>
        /// Percent-encode a given input string.
        /// </summary>
        /// <param name="input">The input string to percent-encode.</param>
        /// <param name="encoding">Encoding used to encode characters.</param>
        /// <param name="additionalReservedChars">Unreserved characters based on RFC 3986, but need to be encoded as well</param>
        /// <returns>Percent encoded string.</returns>
        public static string Encode(string input, Encoding encoding, char[] additionalReservedChars = default(char[]))
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            
            var unreservedCharacters = BuildUnreservedCharacters(additionalReservedChars);
            var resultBuilder = new StringBuilder(input.Length);

            var index = 0;
            while (index < input.Length)
            {
                var c = input[index];
                if (IsUnreservedCharacter(c, unreservedCharacters))
                {
                    // unreserved character, which does not need to be percent encoded
                    resultBuilder.Append(c);
                    index++;
                }
                else
                {
                    // reserved character, but encoding needs to be applied first
                    StringBuilder sb = new StringBuilder().Append(c);
                    index++;
                    while (index < input.Length && !IsUnreservedCharacter(input[index], unreservedCharacters))
                    {
                        sb.Append(input[index]);
                        index++;
                    }
                    // encode temp string using given encoding & percent encoding
                    try
                    {
                        var encoded = encoding.GetBytes(sb.ToString());
                        encoded.ForEach(encodedByte => resultBuilder.AppendFormat("%{0:X2}", encodedByte));
                    }
                    catch (EncoderFallbackException)
                    {
                        // should not be reached
                        return null;
                    }
                }
            }

            return resultBuilder.ToString();
        }

        /// <summary>
        /// Build unreserved characters.
        /// </summary>
        /// <param name="additionalReservedChars">Additional characters to consider as reserved.</param>
        /// <returns>Bitset where all reserved characters are set.</returns>
        private static BitArray BuildUnreservedCharacters(char[] additionalReservedChars)
        {
            var unreservedCharacters = UNRESERVED_CHARACTERS;
            if (additionalReservedChars != null && additionalReservedChars.Length > 0)
            {
                // duplicate unreserved characters
                unreservedCharacters = new BitArray(UNRESERVED_CHARACTERS);
                additionalReservedChars
                    .Where(c => c < unreservedCharacters.Length)
                    .ForEach(c => unreservedCharacters[c] = false);
            }

            return unreservedCharacters;
        }

        /// <summary>
        /// Test if the given character <code>c</code> is unreserved.
        /// </summary>
        /// <param name="c">The character to test, whether it's unreserved or not.</param>
        /// <param name="unreservedCharacters">Unreserved characters</param>
        /// <returns><code>true</code> if it is an unreserved character, <code>false</code> otherwise.</returns>
        private static bool IsUnreservedCharacter(char c, BitArray unreservedCharacters)
        {
            return c < unreservedCharacters.Length && unreservedCharacters[c];
        }
    }
}
