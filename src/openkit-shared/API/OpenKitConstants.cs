namespace Dynatrace.OpenKit.API
{
    /// <summary>
    /// Defines constant values used in OpenKit
    /// </summary>
    public class OpenKitConstants
    {
        // default values used in configuration
        public const string DEFAULT_APPLICATION_VERSION = "0.4";
        public const string DEFAULT_OPERATING_SYSTEM = "OpenKit " + DEFAULT_APPLICATION_VERSION;
        public const string DEFAULT_MANUFACTURER = "Dynatrace";
        public const string DEFAULT_MODEL_ID = "OpenKitDevice";

        // Name of Dynatrace HTTP header which is used for tracing web requests.
        public const string WEBREQUEST_TAG_HEADER = "X-dynaTrace";
    }
}
