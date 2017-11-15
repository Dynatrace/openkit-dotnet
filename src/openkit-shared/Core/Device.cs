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
        private string operatingSystem = null;
        private string manufacturer = null;
        private string modelID = null;

        // *** IDevice interface methods & propreties ***

        public string OperatingSystem {
            get {
                return operatingSystem;
            }
            set {
                this.operatingSystem = value;
            }
        }

        public string Manufacturer {
            get {
                return manufacturer;
            }
            set {
                this.manufacturer = value;
            }
        }

        public string ModelID {
            get {
                return modelID;
            }
            set {
                this.modelID = value;
            }
        }
        
    }

}