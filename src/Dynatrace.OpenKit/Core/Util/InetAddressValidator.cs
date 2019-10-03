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
using System.Text.RegularExpressions;

namespace Dynatrace.OpenKit.Core.Util
{

    /// <summary>
    ///  Utility to check if a InetAddress is valid
    /// </summary>
    public static class InetAddressValidator
    {
        //
        // 4 blocks of number ranging from 0-255
        // the first part of the regex checks the first block
        // the second block checks that there are three further blocks prepended by a '.'
        //
        private static readonly Regex IpV4Regex = new Regex( "^"          // start of string
                            + "(25[0-5]|2[0-4]\\d|[0-1]?\\d?\\d)"         // first block - a number from 0-255
                            + "(\\.(25[0-5]|2[0-4]\\d|[0-1]?\\d?\\d)){3}" // three more blocks - numbers from 0-255 - each prepended by a point character '.'
                            +"$"                                          // end of string
                            , RegexOptions.ECMAScript);

        private static readonly Regex IpV6Regex = new Regex("^"           // start of string
                            + "(?:[0-9a-fA-F]{1,4}:){7}"                  // 7 blocks of a 1 to 4 digit hex number followed by double colon ':'
                            + "[0-9a-fA-F]{1,4}"                          // one more block of a 1 to 4 digit hex number
                            + "$"                                         // end of string
                            , RegexOptions.ECMAScript);

        private static readonly Regex IpV6HexCompressedRegex   = new Regex("^" // start of string
                            + "("                                              // 1st group
                            + "(?:[0-9A-Fa-f]{1,4}"                            // at least one block of a 1 to 4 digit hex number
                            + "(?::[0-9A-Fa-f]{1,4})*)?"                       // optional further blocks, any number
                            + ")"
                            + "::"                                             // in the middle of the expression the two occurrences of ':' are necessary
                            + "("                                              // 2nd group
                            + "(?:[0-9A-Fa-f]{1,4}"                            // at least one block of a 1 to 4 digit hex number
                            + "(?::[0-9A-Fa-f]{1,4})*)?"                       // optional further blocks, any number
                            + ")"
                            + "$"                                              // end of string
                            , RegexOptions.ECMAScript);

        //this regex checks the ipv6 uncompressed part of a ipv6 mixed address
        private static readonly Regex IpV6MixedCompressedRegex = new Regex("^"  // start of string
                            + "("                                               // 1st group
                            + "(?:[0-9A-Fa-f]{1,4}"                             // at least one block of a 1 to 4 digit hex number
                            + "(?::[0-9A-Fa-f]{1,4})*)?"                        // optional further blocks, any number
                            + ")"
                            + "::"                                              // in the middle of the expression the two occurrences of ':' are necessary
                            + "("                                               // 2nd group
                            + "(?:[0-9A-Fa-f]{1,4}:"                            // at least one block of a 1 to 4 digit hex number followed by a ':' character
                            + "(?:[0-9A-Fa-f]{1,4}:)*)?"                        // optional further blocks, any number, all succeeded by ':' character
                            + ")"
                            + "$"                                               // end of string
                            , RegexOptions.ECMAScript);

        //this regex checks the ipv6 uncompressed part of a ipv6 mixed address
        private static readonly Regex IpV6MixedUncompressedRegex = new Regex("^" + // start of string
                            "(?:[0-9a-fA-F]{1,4}:){6}"                             // 6 blocks of a 1 to 4 digit hex number followed by double colon ':'
                            + "$"                                                  // end of string
                            , RegexOptions.ECMAScript);


        ///<summary>
        /// Check if <code>input</code> is a valid IPv4 address
        ///  The format is 'xxx.xxx.xxx.xxx'. Four blocks of integer numbers ranging from 0 to 255
        ///  are required. Letters are not allowed.
        ///</summary>
        ///<param name="input">ip-address to check</param>
        ///<returns> true if input is in correct IPv4 notation.</returns>
        public static bool IsIPv4Address(string input)
        {
            return IpV4Regex.Match(input).Success;
        }


        ///<summary>
        /// Check if the given address is a valid IPv6 address in the standard format
        ///  The format is 'xxxx:xxxx:xxxx:xxxx:xxxx:xxxx:xxxx:xxxx'. Eight blocks of hexadecimal digits
        ///  are required.
        /// </summary>
        ///<param name="input">ip-address to check</param>
        ///<returns>true if input is in correct IPv6 notation.</returns>
        public static bool IsIPv6StdAddress(string input)
        {
            return IpV6Regex.Match(input).Success;
        }

        ///<summary>
        /// Check if the given address is a valid IPv6 address in the hex-compressed notation
        ///  The format is 'xxxx:xxxx:xxxx:xxxx::xxxx:xxxx'. The longest consecutive block
        ///  of '0's can be replaced by '::'
        ///</summary>
        ///<param name="input">ip-address to check</param>
        ///<returns>true if input is in correct IPv6 (hex-compressed) notation</returns>
        public static bool IsIPv6HexCompressedAddress(string input)
        {
            return IpV6HexCompressedRegex.Match(input).Success;
        }

        ///<summary>
        /// Check if <code>input</code> is a IPv6 address.
        /// Possible notations for valid IPv6 are:
        ///   Standard IPv6 address
        ///   Hex-compressed IPv6 address
        ///   Link-local IPv6 address
        ///   IPv4-mapped-to-IPV6 address
        ///   iPv6 mixed address
        /// </summary>
        /// <param name="input">ip-address to check</param>
        /// <returns>true if input is in correct IPv6 notation</returns>
        public static bool IsIPv6Address(string input)
        {
            return IsIPv6StdAddress(input) || IsIPv6HexCompressedAddress(input) || IsLinkLocalIPv6WithZoneIndex(input)
                    || IsIPv6IPv4MappedAddress(input) || IsIPv6MixedAddress(input);
        }


        ///<summary>
        /// Check if the given address is a valid IPv6 address in the mixed-standard or mixed-compressed notation.
        ///</summary>
        /// <param name="input">input ip-address to check</param>
        /// <returns>@return true if input is in correct IPv6 (mixed-standard or mixed-compressed) notation.</returns>
        ///
        public static bool IsIPv6MixedAddress(string input)
        {
            var splitIndex = input.LastIndexOf(':');

            if (splitIndex == -1)
                return false;

            //the last part is a ipv4 adress
            var ipv4PartValid = IsIPv4Address(input.Substring(splitIndex + 1 ));

            var ipV6Part = input.Substring(0, splitIndex + 1 );
            if(ipV6Part.CompareTo("::") == 0)
            {
                return ipv4PartValid;
            }

            var ipV6UncompressedDetected = IpV6MixedUncompressedRegex.Match(ipV6Part).Success;
            var ipV6CompressedDetected = IpV6MixedCompressedRegex.Match(ipV6Part).Success;

            return ipv4PartValid && (ipV6UncompressedDetected || ipV6CompressedDetected);
        }


        ///<summary>
        /// Check if <code>input</code> is an IPv4 address mapped into a IPv6 address. These are
        /// starting with "::ffff:" followed by the IPv4 address in a dot-separated notation.
        /// </summary>
        /// <param name="input">ip-address to check </param>
        /// <returns>true if input is in correct IPv6 notation</returns>
        public static bool IsIPv6IPv4MappedAddress(string input)
        {
            // InetAddress automatically convert this type of address down to an IPv4 address
            // It always starts '::ffff:' then contains an IPv4 address
            if (input.Length > 7 && input.Substring(0, 7).Equals("::ffff:", StringComparison.OrdinalIgnoreCase))
            {
                // then remove the first seven chars and see if we have an IPv4 address
                string lowerPart = input.Substring(7);
                return IsIPv4Address(lowerPart);
            }
            return false;
        }

        ///<summary>
        /// Check if <code>input</code> is a link local IPv6 address starting with "fe80:" and containing
        /// a zone index with "%xxx". The zone index will not be checked.
        /// </summary>
        /// <param name="input">InetAddress to check </param>
        /// <returns>true if address part of input is in correct IPv6 notation</returns>
        public static bool IsLinkLocalIPv6WithZoneIndex(string input)
        {
            if (input.Length > 5 && input.Substring(0, 5).Equals("fe80:", StringComparison.OrdinalIgnoreCase))
            {
                int lastIndex = input.LastIndexOf("%");
                if (lastIndex > 0 && lastIndex < (input.Length - 1))
                { // input may not start with the zone separator
                    string ipPart = input.Substring(0, lastIndex);
                    return IsIPv6StdAddress(ipPart) || IsIPv6HexCompressedAddress(ipPart);
                }
            }
            return false;
        }

        /// <summary>
        /// Check if ipAddress is a valid IPv4 or IPv6 address
        /// </summary>
        /// <param name="ipAddress">ip-address tp check</param>
        /// <returns>true if ipAddress is a valid IPv4 or IPv6 address </returns>
        public static bool IsValidIP(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                return false;
            }

            return IsIPv4Address(ipAddress) || IsIPv6Address(ipAddress);
        }

    }

}