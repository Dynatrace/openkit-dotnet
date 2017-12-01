/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;
using Dynatrace.OpenKit.Core.Configuration;

namespace Dynatrace.OpenKit {

    /// <summary>
    ///  This factory creates instances of the OpenKit to work with.
    ///  It can be used to create instances for both Dynatrace AppMon and Dynatrace SaaS/Managed.
    /// </summary>
    public class OpenKitFactory
    {

        /// <summary>
        ///  Name of Dynatrace HTTP header which is used for tracing web requests.
        /// </summary>
        public const string WEBREQUEST_TAG_HEADER = "X-dynaTrace";

        /// <summary>
        ///  Creates a Dynatrace SaaS instance of the OpenKit.
        /// </summary>
        /// <param name="applicationName">the application name</param>
        /// <param name="applicationID">the application ID (must be a valid application UUID)</param>
        /// <param name="visitorID">unique visitor ID</param>
        /// <param name="endpointURL">the URL of the beacon forwarder to send the data to</param>
        /// <returns>Dynatrace SaaS instance of the OpenKit</returns>
        public static IOpenKit CreateDynatraceInstance(string applicationName, string applicationID, long visitorID, string endpointURL)
        {
            return CreateDynatraceInstance(applicationName, applicationID, visitorID, endpointURL, false);
        }

        /// <summary>
        ///  Creates a Dynatrace SaaS instance of the OpenKit, optionally with verbose logging.
        /// </summary>
        /// <param name="applicationName">the application name</param>
        /// <param name="applicationID">the application ID (must be a valid application UUID)</param>
        /// <param name="visitorID">unique visitor ID</param>
        /// <param name="endpointURL">the URL of the beacon forwarder to send the data to</param>
        /// <param name="verbose">if true, turn on verbose logging on stdout</param>
        /// <returns>Dynatrace SaaS instance of the OpenKit</returns>
        public static IOpenKit CreateDynatraceInstance(string applicationName, string applicationID, long visitorID, string endpointURL, bool verbose)
        {
            var openKit = new Core.OpenKit(new DynatraceConfiguration(applicationName, applicationID, visitorID, endpointURL, verbose));
            openKit.Initialize();

            return openKit;
        }

        /// <summary>
        ///  Creates a Dynatrace Managed instance of the OpenKit.
        /// </summary>
        /// <param name="applicationName">the application name</param>
        /// <param name="applicationID">the application ID (must be a valid application UUID)</param>
        /// <param name="visitorID">unique visitor ID</param>
        /// <param name="endpointURL">the URL of the beacon forwarder to send the data to</param>
        /// <param name="tenantID">the id of the tenant</param>
        /// <returns>Dynatrace Managed instance of the OpenKit</returns>
        public static IOpenKit CreateDynatraceManagedInstance(string applicationName, string applicationID, long visitorID, string endpointURL, string tenantID)
        {
            return CreateDynatraceManagedInstance(applicationName, applicationID, visitorID, endpointURL, tenantID, false);
        }

        /// <summary>
        ///  Creates a Dynatrace Managed instance of the OpenKit, optionally with verbose logging.
        /// </summary>
        /// <param name="applicationName">the application name</param>
        /// <param name="applicationID">the application ID (must be a valid application UUID)</param>
        /// <param name="visitorID">unique visitor ID</param>
        /// <param name="endpointURL">the URL of the beacon forwarder to send the data to</param>
        /// <param name="tenantID">the id of the tenant</param>
        /// <param name="verbose">if true, turn on verbose logging on stdout</param>
        /// <returns>Dynatrace Managed instance of the OpenKit</returns>
        public static IOpenKit CreateDynatraceManagedInstance(string applicationName, string applicationID, long visitorID, string endpointURL, string tenantID, bool verbose)
        {
            var openKit = new Core.OpenKit(new DynatraceManagedConfiguration(tenantID, applicationName, applicationID, visitorID, endpointURL, verbose));
            openKit.Initialize();

            return openKit;
        }

        /// <summary>
        ///  Creates a Dynatrace AppMon instance of the OpenKit.
        /// </summary>
        /// <param name="applicationName">the application name</param>
        /// <param name="visitorID">unique visitor ID</param>
        /// <param name="endpointURL">the URL of the Java/Webserver Agent to send the data to</param>
        /// <returns>Dynatrace AppMon instance of the OpenKit</returns>
        public static IOpenKit CreateAppMonInstance(string applicationName, long visitorID, string endpointURL)
        {
            return CreateAppMonInstance(applicationName, visitorID, endpointURL, false);
        }

        /// <summary>
        ///  Creates a Dynatrace AppMon instance of the OpenKit, optionally with verbose logging.
        /// </summary>
        /// <param name="applicationName">the application name</param>
        /// <param name="visitorID">unique visitor ID</param>
        /// <param name="endpointURL">the URL of the Java/Webserver Agent to send the data to</param>
        /// <param name="verbose">if true, turn on verbose logging on stdout</param>
        /// <returns>Dynatrace AppMon instance of the OpenKit</returns>
        public static IOpenKit CreateAppMonInstance(string applicationName, long visitorID, string endpointURL, bool verbose)
        {
            var openKit = new Core.OpenKit(new AppMonConfiguration(applicationName, visitorID, endpointURL, verbose));
            openKit.Initialize();

            return openKit;
        }
    }
}
