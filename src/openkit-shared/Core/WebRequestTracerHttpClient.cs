/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core
{
#if (!NET40 && !NET35)
    /// <summary>
    ///  Inherited class of WebRequestTracerBase which can be used for tracing and timing of a web request provided via an HttpClient.
    /// </summary>
    public class WebRequestTracerHttpClient : WebRequestTracerBase
    {

        // *** constructors ***

        // creates web request tracer with a System.Net.Http.HttpClient
        public WebRequestTracerHttpClient(Beacon beacon, Action action, System.Net.Http.HttpClient httpClient) : base(beacon, action)
        {
            if (httpClient != null)
            {
                SetTagOnConnection(httpClient);

                this.url = httpClient.BaseAddress.AbsoluteUri;
            }
        }

        // *** private methods ***

        // set the Dynatrace tracer on the provided URLConnection
        private void SetTagOnConnection(System.Net.Http.HttpClient httpClient)
        {
            // check if header is already set
            if (!httpClient.DefaultRequestHeaders.Contains(OpenKitConstants.WEBREQUEST_TAG_HEADER))
            {
                // if not yet set -> set it now
                httpClient.DefaultRequestHeaders.Add(OpenKitConstants.WEBREQUEST_TAG_HEADER, Tag);
            }
        }

    }

#endif

}
