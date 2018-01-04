/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Johannes Bäuerle
 */
using System;
using System.Text.RegularExpressions;

namespace Dynatrace.OpenKit.Core.Util
{

    /// <summary>
    ///  Utility to check if a InetAddress is valid
    /// </summary>
    public class InetAddressValidator
    {
        private static readonly Regex IpV4Regex = new Regex("^(25[0-5]|2[0-4]\\d|[0-1]?\\d?\\d)(\\.(25[0-5]|2[0-4]\\d|[0-1]?\\d?\\d)){3}$");

        private static readonly Regex IpV6Regex = new Regex("^(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$");

        private static readonly Regex IpV6HexCompressedRegex = new Regex("^((?:[0-9A-Fa-f]{1,4}(?::[0-9A-Fa-f]{1,4})*)?)::((?:[0-9A-Fa-f]{1,4}(?::[0-9A-Fa-f]{1,4})*)?)$");

        private static readonly Regex IpV6MixedRegex = new Regex("(?ix)(?<![:.\\w])                                     # Anchor address\n" +
                            "(?:\n" +
                            " (?:[A-F0-9]{1,4}:){6}                                # Non-compressed\n" +
                            "|(?=(?:[A-F0-9]{0,4}:){2,6}                           # Compressed with 2 to 6 colons\n" +
                            "    (?:[0-9]{1,3}\\.){3}[0-9]{1,3}                    #    and 4 bytes\n" +
                            "    (?![:.\\w]))                                      #    and anchored\n" +
                            " (([0-9A-F]{1,4}:){1,5}|:)((:[0-9A-F]{1,4}){1,5}:|:)  #    and at most 1 double colon\n" +
                            "|::(?:[A-F0-9]{1,4}:){5}                              # Compressed with 7 colons and 5 numbers\n" +
                            ")\n" +
                            "(?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\\.){3}  # 255.255.255.\n" +
                            "(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])       # 255\n" +
                            "(?![:.\\w])                                           # Anchor address");



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
        ///<param name="input">ip-adress to check</param>
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
        ///<param name="input">ip-adress to check</param>
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
        /// <param name="input">ip-adress to check</param>
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
            return IpV6MixedRegex.Match(input).Success;
        }


        ///<summary>
        /// Check if <code>input</code> is an IPv4 address mapped into a IPv6 address. These are
        /// starting with "::fff:" followed by the IPv4 address in a dot-seperated notation.
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
        /// <param name="input">InetAdress to check </param>
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
        /// <returns>true if ipAdress is a valid IPv4 or IPv6 address </returns>
        public static bool IsValidIP(string ipAddress)
        {
            if (ipAddress == null || ipAddress.Length == 0)
            {
                return false;
            }

            return IsIPv4Address(ipAddress) || IsIPv6Address(ipAddress);
        }

    }

}