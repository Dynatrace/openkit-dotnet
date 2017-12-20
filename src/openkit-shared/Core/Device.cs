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

        // platform information
        private string operatingSystem = OpenKitConstants.DEFAULT_OPERATING_SYSTEM;
        private string manufacturer = OpenKitConstants.DEFAULT_MANUFACTURER;
        private string modelID = OpenKitConstants.DEFAULT_DEVICE_ID;

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