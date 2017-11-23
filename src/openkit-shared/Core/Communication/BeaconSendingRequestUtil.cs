using Dynatrace.OpenKit.Protocol;

namespace Dynatrace.OpenKit.Core.Communication
{
    internal static class BeaconSendingRequestUtil
    {
        internal static StatusResponse SendStatusRequest(IBeaconSendingContext context, int numRetries, int initialRetryDelayInMillis)
        {
            StatusResponse statusResponse = null;
            var sleepTimeInMillis = initialRetryDelayInMillis;
            var retry = 0;

            while (true)
            {
                statusResponse = context.GetHTTPClient().SendStatusRequest();
                if (statusResponse != null || retry >= numRetries || context.IsShutdownRequested)
                {
                    break;
                }

                // if no (valid) status response was received -> sleep and double the delay for each retry
                context.Sleep(sleepTimeInMillis);
                sleepTimeInMillis *= 2;
                retry++;
            }

            return statusResponse;
        }
    }
}
