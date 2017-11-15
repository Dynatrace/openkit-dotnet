/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
namespace Dynatrace.OpenKit.API {

    /// <summary>
    ///  This interface provides functionality to set basic device information, like operating system, manufacturer and model information.
    /// </summary>
    public interface IDevice {

        /// <summary>
        ///  Sets operating system name.
        /// </summary>
        string OperatingSystem { set; }

        /// <summary>
        ///  Sets manufacturer name.
        /// </summary>
        string Manufacturer { set; }

        /// <summary>
        ///  Sets a model identifier.
        /// </summary>
        string ModelID { set; }

    }
}