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
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Protocol.SSL
{
    /// <summary>
    /// Implementation of <see cref="ISSLTrustManager"/> blindly trusting every certificate and every host.
    /// </summary>
    ///
    /// <remarks>
    /// This class is intended to be used only during development phase. Since local
    /// development environments use self-signed certificates only.
    ///
    /// This implementation disables any X509 certificate validation & hostname validation.
    ///
    /// NOTE: DO NOT USE THIS IN PRODUCTION!!
    /// </remarks>
#if !(WINDOWS_UWP || NETSTANDARD1_1)
    public class SSLBlindTrustManager : ISSLTrustManager
    {
        public SSLBlindTrustManager()
        {
            Console.WriteLine("###########################################################");
            Console.WriteLine("# WARNING: YOU ARE BYPASSING SSL CERTIFICATE VALIDATION!! #");
            Console.WriteLine("#                 USE AT YOUR OWN RISK!!                  #");
            Console.WriteLine("###########################################################");
        }

        public System.Net.Security.RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get
            {
                return (sender, certificate, chain, sslPolicyErrors) => true;
            }
        }
    }
#endif
}
