//
// Copyright 2018-2020 Dynatrace LLC
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

using Dynatrace.OpenKit;
using Dynatrace.OpenKit.API;
using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Samples
{
    /// <summary>
    /// This class can be used on a WCF client to automatically add the headers required for
	/// tracing WCF (Windows Communication Foundation) service calls.
    /// </summary>
	public class TraceServiceCallBehavior : IEndpointBehavior
    {
        private readonly IAction action;

        public TraceServiceCallBehavior(IAction action)
        {
            this.action = action;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // intentionally left empty
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new ClientMessageInspector(action, endpoint.Address.Uri.ToString()));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // intentionally left empty
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // intentionally left empty
        }

        /// <summary>
        /// Class used to intercept a message before it gets send. A custom HTTP header is added before sending the message.
        /// This class is also used to intercept a WCF service reply to read the HTTP response code.
        /// </summary>
        private class ClientMessageInspector : IClientMessageInspector
        {
            private readonly IAction action;
            private readonly string endpoint;
            private IWebRequestTracer webRequestTracer;

            public ClientMessageInspector(IAction action, string endpoint)
            {
                this.action = action;
                this.endpoint = endpoint;
            }

            public void AfterReceiveReply(ref Message reply, object correlationState)
            {
                if (webRequestTracer != null)
                {
                    object httpResponseMessageObject;
                    int responseCode;
                    if (reply.Properties.TryGetValue(HttpResponseMessageProperty.Name, out httpResponseMessageObject))
                    {
                        var httpResponseMessage = httpResponseMessageObject as HttpResponseMessageProperty;
                        responseCode = (int)httpResponseMessage.StatusCode;
                    }
                    else
                    {
                        // assume HTTP OK by default
                        responseCode = (int)HttpStatusCode.OK
                    }

                    webRequestTracer.Stop(responseCode);
                    webRequestTracer = null;
                }
            }

            public object BeforeSendRequest(ref Message request, IClientChannel channel)
            {
                // get the request URI
                var requestUri = GetRequestUri(request);
                webRequestTracer = action.TraceWebRequest(requestUri);
                webRequestTracer.Start();

                // add Dynatrace custom header for tracing
                object httpRequestMessageObject;
                if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
                {
                    var httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;

                    if (string.IsNullOrWhiteSpace(httpRequestMessage.Headers[OpenKitConstants.WEBREQUEST_TAG_HEADER]))
                    {
                        httpRequestMessage.Headers[OpenKitConstants.WEBREQUEST_TAG_HEADER] = webRequestTracer.Tag;
                    }
                }
                else
                {
                    // request properties does not contain request message properties - just for safety reasons
                    var httpRequestMessage = new HttpRequestMessageProperty();
                    httpRequestMessage.Headers.Add(OpenKitConstants.WEBREQUEST_TAG_HEADER, webRequestTracer.Tag);

                    request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
                }

                return null;
            }

            /// <summary>
            /// Get the full URI of the WCF service call.
            /// </summary>
            private string GetRequestUri(Message request)
            {
                string requestUri = null;
                if (!IsSoapMessage(request))
                {
                    // not a SOAP envelope
                    requestUri = request.Headers.To?.AbsoluteUri;
                }
                else
                {
                    // not matter which SOAP version, it's a SOAP envelope
                    requestUri = request.Headers.Action;
                }

                return requestUri ?? endpoint;
            }

            private static bool IsSoapMessage(Message request)
            {
                return !request.Headers.MessageVersion.Envelope.Equals(EnvelopeVersion.None);
            }
        }
    }

    /// <summary>
    /// The WcfAutoTracerSample includes a basic example that provides an overview of the features supported by OpenKit.
    /// For more detailed information, please refer to the documentation that is available on GitHub.
    /// </summary>
    public class WcfAutoTracerSample
    {
        private const string WebServiceBaseAddress = "http://my.wcf-service.com:1234/service";

        public static void Main(string[] args)
        {
            string endpointURL = "";    // the endpointURL can be found in the Dynatrace UI
            string applicationID = "";  // the application id can be found in the Dynatrace UI
            long deviceID = 42L;        // an ID that uniquely identifies the device

            var openKit = new DynatraceOpenKitBuilder(endpointURL, applicationID, deviceID)
                .WithApplicationVersion("1.0")
                .WithOperatingSystem(Environment.OSVersion.VersionString)
                .Build();

            // we wait for OpenKit to be initialized
            // if you skip the line, OpenKit will be initialized asynchronously
            openKit.WaitForInitCompletion();

            // create a new session
            var session = openKit.CreateSession("127.0.0.1");

            // create a new Action which is passed to an IWebRequestTracer instance
            var tracingAction = session.EnterAction("WCF Tracing Action");

            sendWcfServiceCall(tracingAction);

            // leave the tracing action
            tracingAction.LeaveAction();

            // end session
            session.End();

            // shutdown OpenKit
            openKit.Shutdown();
        }

        /// <summary>
        /// Method for performing a WCF call.
        /// </summary>
        private static void sendWcfServiceCall(IAction tracingAction)
        {
            using (var webChannelFactory = new WebChannelFactory<IService>(new Uri(WebServiceBaseAddress)))
            {
                // Hint: Add an instance of TraceServiceCallBehavior to the client's endpoint behaviors
                // If the following line is omitted, no automatic tracing is performed.
                webChannelFactory.Endpoint.EndpointBehaviors.Add(new TraceServiceCallBehavior(tracingAction));

                var channel = webChannelFactory.CreateChannel();


                Console.WriteLine("Calling ServiceMethod by HTTP GET: ");
                var response = channel.ServiceMethod("Hello, world!");
                Console.WriteLine($"   Output: {response}");
            }
        }
    }


    /// <summary>
    /// WCF service contract.
    /// </summary>
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        [WebGet]
        string ServiceMethod(string argument);
    }
}
