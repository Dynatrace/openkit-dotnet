namespace Dynatrace.OpenKit.API
{
    /// <summary>
    ///  This interface provides the same functionality as IAction and in addition allows to create child actions
    /// </summary>
    public interface IRootAction : IAction
    {
        /// <summary>
        ///  Enters a child Action with a specified name on this Action.
        /// </summary>
        /// <param name="actionName">name of the Action</param>
        /// <returns>Action instance to work with</returns>
        IAction EnterAction(string actionName);
    }
}
