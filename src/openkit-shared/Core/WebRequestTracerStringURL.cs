/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.Protocol;
using System;

namespace Dynatrace.OpenKit.Core
{

    /// <summary>
    ///  Inherited class of WebRequestTracerBase which can be used for tracing and timing of a web request handled by any 3rd party HTTP Client.
    ///  Setting the Dynatrace tag to the OpenKit.WEBREQUEST_TAG_HEADER HTTP header has to be done manually by the user.
    /// </summary>
    public class WebRequestTracerStringURL : WebRequestTracerBase
    {

        // *** constructors ***

        public WebRequestTracerStringURL(Beacon beacon, Action action, string url) : base(beacon, action)
        {
            this.url = url;

            // separate query string from URL
            if (url != null)
            {
                this.url = url.Split(new Char[] { '?' })[0];
            }
        }

    }

}
