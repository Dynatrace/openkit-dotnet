namespace Dynatrace.OpenKit.Core.Objects
{
    /// <summary>
    /// Containing constants which can be overridden in the sendEvent API
    /// </summary>
    internal static class EventPayloadAttributes
    {
        internal const string TIMESTAMP = "timestamp";
        internal const string EVENT_KIND = "event.kind";
        internal const string DT_AGENT_VERSION = "dt.agent.version";
        internal const string DT_AGENT_TECHNOLOGY_TYPE = "dt.agent.technology_type";
        internal const string DT_AGENT_FLAVOR = "dt.agent.flavor";
        internal const string APP_VERSION = "app.version";
        internal const string OS_NAME = "os.name";
        internal const string DEVICE_MANUFACTURER = "device.manufacturer";
        internal const string DEVICE_MODEL_IDENTIFIER = "device.model.identifier";
        internal const string WINDOW_ORIENTATION = "window.orientation";

        internal const string EVENT_KIND_RUM = "RUM_EVENT";
        internal const string EVENT_KIND_BIZ = "BIZ_EVENT";
    }
}
