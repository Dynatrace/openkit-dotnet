namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// Containing constants which can be overridden in the sendEvent API
    /// </summary>
    internal static class EventPayloadAttributes
    {
        internal const string TIMESTAMP = "timestamp";
        internal const string EVENT_KIND = "event.kind";
        internal const string APP_VERSION = "app.version";
        internal const string OS_NAME = "os.name";
        internal const string DEVICE_MANUFACTURER = "device.manufacturer";
        internal const string DEVICE_MODEL_IDENTIFIER = "device.model.identifier";
        internal const string EVENT_PROVIDER = "event.provider";
        internal const string EVENT_KIND_RUM = "RUM_EVENT";
        internal const string EVENT_KIND_BIZ = "BIZ_EVENT";
    }
}
