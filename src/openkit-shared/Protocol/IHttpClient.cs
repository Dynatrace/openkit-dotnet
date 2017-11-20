namespace Dynatrace.OpenKit.Protocol
{
    public interface IHTTPClient
    {
        /// <summary>
        /// Sends a status request and returns a status response
        /// </summary>
        /// <returns></returns>
        StatusResponse SendStatusRequest();
        
        /// <summary>
        /// Sends a beacon send request and returns a status response
        /// </summary>
        /// <param name="clientIPAddress"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        StatusResponse SendBeaconRequest(string clientIPAddress, byte[] data);

        /// <summary>
        /// Sends a time sync request and returns a time sync response
        /// </summary>
        /// <returns></returns>
        TimeSyncResponse SendTimeSyncRequest();
    }
}
