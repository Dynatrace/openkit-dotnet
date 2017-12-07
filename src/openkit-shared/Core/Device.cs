/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core {

    /// <summary>
    ///  Actual implementation of the IDevice interface.
    /// </summary>
    public class Device : IDevice {

        //defaults for OpenKit devices
        private const string DEFAULT_OPERATING_SYSTEM = "OpenKit 0.3";
        private const string DEFAULT_MANUFACTURER = "Dynatrace";
        private const string DEFAULT_DEVICE_ID = "OpenKitDevice";

        // platform information
        private string operatingSystem = DEFAULT_OPERATING_SYSTEM;
        private string manufacturer = DEFAULT_MANUFACTURER;
        private string modelID = DEFAULT_DEVICE_ID;

        // *** IDevice interface methods & propreties ***

        public string OperatingSystem {
            get {
                return operatingSystem;
            }
            set {
                if(!string.IsNullOrEmpty(value))
                {
                    this.operatingSystem = value;
                }
            }
        }

        public string Manufacturer {
            get {
                return manufacturer;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.manufacturer = value;
                }
            }
        }

        public string ModelID {
            get {
                return modelID;
            }
            set {
                if (!string.IsNullOrEmpty(value))
                {
                    this.modelID = value;
                }
            }
        }
        
    }

}