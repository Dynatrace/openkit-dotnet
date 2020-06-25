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
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Samples
{
    /// <summary>
    /// The SimpleSample includes a basic example that provides an overview of the features supported by OpenKit.
    /// For more detailed information, please refer to the documentation that is available on GitHub.
    /// </summary>
    public class SimpleSample
    {
        public static void Main(string[] args)
        {
            string endpointURL = "";    // the endpointURL can be found in the Dynatrace UI
            string applicationID = "";  // the application id can be found in the Dynatrace UI
            long deviceID = 42L;        // an ID that uniquely identifies the device

            var openKit = new DynatraceOpenKitBuilder(endpointURL, applicationID, deviceID)
                .WithApplicationName("SimpleSampleApp")
                .WithApplicationVersion("1.0")
                .WithOperatingSystem(Environment.OSVersion.VersionString)
                .Build();

            // we wait for OpenKit to be initialized
            // if you skip the line, OpenKit will be initialized asynchronously
            openKit.WaitForInitCompletion();

            // create a new session
            var session = openKit.CreateSession("127.0.0.1");

            // identify the user
            session.IdentifyUser("openKitExampleUser");

            // create a root action
            var rootAction = session.EnterAction("talk to postman");

            // execute a GET request to the postman echo API and trace request time and size
            ExecuteAndTraceWebRequestAsync(rootAction, "https://postman-echo.com/get?query=users").Wait();

            // wait a bit
            Thread.Sleep(1000);

            // execute a POST request to the postman echo API and trace request time and size
            ExecuteAndTraceWebRequestAsync(rootAction, "https://postman-echo.com/post", "This is content that we want to be processed by the server").Wait();

            // report a value representing the desired sleep time
            rootAction.ReportValue("sleepTime", 2000);

            // create a child action
            var childAction = rootAction.EnterAction("sleeping");

            // wait again
            Thread.Sleep(2000);

            // report an event indicating that we finished sleeping
            rootAction.ReportEvent("finished sleeping");

            // leave both actions
            childAction.LeaveAction();
            rootAction.LeaveAction();

            // end session
            session.End();

            // shutdown OpenKit
            openKit.Shutdown();
        }

        private static async System.Threading.Tasks.Task ExecuteAndTraceWebRequestAsync(IAction action, string endpoint, string payload = null)
        {
            using (var httpClient = new HttpClient())
            {
                var bytesToSend = string.IsNullOrEmpty(payload) ? 0 : Encoding.Default.GetByteCount(payload);

                // get the tracer
                var tracer = action.TraceWebRequest(endpoint);

                // set the tag for the request
                httpClient.DefaultRequestHeaders.Add(OpenKitConstants.WEBREQUEST_TAG_HEADER, tracer.Tag);

                // start timing for web request
                tracer.Start();

                using (var response =
                    await (string.IsNullOrEmpty(payload) ? httpClient.GetAsync(endpoint).ConfigureAwait(false)
                    : httpClient.PostAsync(endpoint, new StringContent(payload)).ConfigureAwait(false)))
                {
                    var bytesReceived = 0;

                    using (var content = response.Content)
                    {
                        var resultBytes = await content.ReadAsByteArrayAsync();

                        // do something useful with the content

                        bytesReceived = resultBytes.Length;
                    }

                    tracer.SetBytesSent(bytesToSend)
                        .SetBytesReceived(bytesReceived)
                        .Stop((int)response.StatusCode);        // stop the tracer
                }
            }
        }
    }
}
