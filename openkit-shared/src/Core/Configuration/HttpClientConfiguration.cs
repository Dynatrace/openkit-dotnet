using System;

namespace Dynatrace.OpenKit.Core.Configuration
{
    /// <summary>
    /// The HttpClientConfiguration holds all http client related settings
    /// </summary>
    public class HttpClientConfiguration
    {
        public HttpClientConfiguration(String baseUrl, int serverId, string applicationId, bool verbose)
        {
            BaseUrl = baseUrl;
            ServerId = serverId;
            ApplicationId = applicationId;
            IsVerbose = verbose;
        }

        /// <summary>
        /// The base Url for the http client
        /// </summary>
        public string BaseUrl { get; private set; }

        /// <summary>
        /// The server id to be used for the http client
        /// </summary>
        public int ServerId { get; private set; }

        /// <summary>
        /// The application id for the http client
        /// </summary>
        public string ApplicationId { get; private set; }

        /// <summary>
        /// If <code>true</code> logging is enabled
        /// </summary>
        public bool IsVerbose { get; private set; }
    }
}
