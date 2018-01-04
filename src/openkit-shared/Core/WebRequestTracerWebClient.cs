/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Stefan Eberl
 */
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core
{
#if NET40 || NET35

    /// <summary>
    ///  Inherited class of WebRequestTracerBase which can be used for tracing and timing of a web request provided via an HttpClient.
    /// </summary>
    public class WebRequestTracerWebClient : WebRequestTracerBase
    {

        // *** constructors ***

        // creates web request tracer with a System.Net.Http.HttpClient
        public WebRequestTracerWebClient(Beacon beacon, Action action, System.Net.WebClient webClient) : base(beacon, action)
        {
            if (webClient != null)
            {
                SetTagOnConnection(webClient);

                this.url = webClient.BaseAddress;
            }
        }

        // *** private methods ***

        // set the Dynatrace tag on the provided URLConnection
        private void SetTagOnConnection(System.Net.WebClient webClient)
        {
            // check if header is already set
            if (webClient.Headers.Get(OpenKitConstants.WEBREQUEST_TAG_HEADER) == null)
            {
                // if not yet set -> set it now
                webClient.Headers.Add(OpenKitConstants.WEBREQUEST_TAG_HEADER, Tag);
            }
        }

    }
#endif
}
