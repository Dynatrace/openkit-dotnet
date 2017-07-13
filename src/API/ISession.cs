/***************************************************
 * (c) 2016-2017 Dynatrace LLC
 *
 * @author: Christian Schwarzbauer
 */
namespace Dynatrace.OpenKit.API {

    /// <summary>
    ///  This interface provides functionality to create Actions in a Session.
    /// </summary>
    public interface ISession {

        /// <summary>
        ///  Enters an Action with a specified name in this Session.
        /// </summary>
        /// <param name="actionName">name of the Action</param>
        /// <returns>Action instance to work with</returns>
        IAction EnterAction(string actionName);

        /// <summary>
        ///  Ends this Session and marks it as finished for sending.
        /// </summary>
        void End();

    }

}
