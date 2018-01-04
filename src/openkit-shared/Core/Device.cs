/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */

namespace Dynatrace.OpenKit.Core
{
    /// <summary>
    ///  Class holding device specific information
    /// </summary>
    public class Device {

        public Device(string operatingSystem, string manufacturer, string modelID)
        {
            OperatingSystem = operatingSystem;
            Manufacturer = manufacturer;
            ModelID = modelID;
        }

        // *** IDevice interface methods & propreties ***

        public string OperatingSystem { get; private set; }

        public string Manufacturer { get; private set; }

        public string ModelID { get; private set; }
        
    }

}