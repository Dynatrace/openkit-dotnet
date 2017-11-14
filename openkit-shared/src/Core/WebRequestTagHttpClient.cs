/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core
{
#if (!NET40 && !NET35)
    /// <summary>
    ///  Inherited class of WebRequestTagBase which can be used for tagging and timing of a web request provided via an HttpClient.
    /// </summary>
    public class WebRequestTagHttpClient : WebRequestTagBase
    {

        // *** constructors ***

        // creates web request tag with a System.Net.Http.HttpClient
        public WebRequestTagHttpClient(Beacon beacon, Action action, System.Net.Http.HttpClient httpClient) : base(beacon, action)
        {
            if (httpClient != null)
            {
                SetTagOnConnection(httpClient);

                this.url = httpClient.BaseAddress.AbsoluteUri;
            }
        }

        // *** private methods ***

        // set the Dynatrace tag on the provided URLConnection
        private void SetTagOnConnection(System.Net.Http.HttpClient httpClient)
        {
            // check if header is already set
            if (!httpClient.DefaultRequestHeaders.Contains(OpenKitFactory.WEBREQUEST_TAG_HEADER))
            {
                // if not yet set -> set it now
                httpClient.DefaultRequestHeaders.Add(OpenKitFactory.WEBREQUEST_TAG_HEADER, Tag);
            }
        }

    }

#endif

}
