namespace Dynatrace.OpenKit.Core.Configuration
{
    public class OpenKitType
    {

        public static readonly OpenKitType APPMON = new OpenKitType("dynaTraceMonitor", 1);         // AppMon: default monitor URL name contains "dynaTraceMonitor" and default Server ID is 1
        public static readonly OpenKitType DYNATRACE = new OpenKitType("mbeacon", -1);              // Dynatrace: default monitor URL name contains "mbeacon" and default Server ID is -1

        private string defaultMonitorName;
        private int defaultServerID;

        OpenKitType(string defaultMonitorName, int defaultServerID)
        {
            this.defaultMonitorName = defaultMonitorName;
            this.defaultServerID = defaultServerID;
        }

        public string DefaultMonitorName
        {
            get
            {
                return defaultMonitorName;
            }
        }

        public int DefaultServerID
        {
            get
            {
                return defaultServerID;
            }
        }

    }
}
