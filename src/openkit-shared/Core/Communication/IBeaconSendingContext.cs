using Dynatrace.OpenKit.Core.Configuration;
using Dynatrace.OpenKit.Protocol;
using Dynatrace.OpenKit.Providers;
using System.Collections.Generic;

namespace Dynatrace.OpenKit.Core.Communication
{
    /// <summary>
    /// State context for beacon sending
    /// </summary>
    internal interface IBeaconSendingContext
    {
        // TODO - thomas.grassauer@dynatrace.com - add documenations

        AbstractConfiguration Configuration { get; }
        IHTTPClientProvider HTTPClientProvider { get; }
        ITimingProvider TimingProvider { get; }

        AbstractBeaconSendingState CurrentState { get; set; }
        long LastOpenSessionBeaconSendTime { get; set; }
        long LastStatusCheckTime { get; set; }
        long LastTimeSyncTime { get; set; }

        bool IsInitialized { get; }
        bool IsTimeSyncSupported { get; }
        bool IsShutdownRequested { get; }
        long CurrentTimestamp { get; }
        int SendInterval { get; }
        bool IsCaptureOn { get; }
        bool IsInTerminalState { get; }

        void DisableTimeSyncSupport();

        void ExecuteCurrentState();

        void RequestShutdown();

        bool WaitForInit();

        bool WaitForInit(int timeoutMillis);

        void InitCompleted(bool success);

        IHTTPClient GetHTTPClient();

        void Sleep();

        void Sleep(int millis);

        void DisableCapture();

        void HandleStatusResponse(StatusResponse statusResponse);

        Session GetNextFinishedSession();

        List<Session> GetAllOpenSessions();

        void StartSession(Session session);

        void FinishSession(Session session);
    }
}
