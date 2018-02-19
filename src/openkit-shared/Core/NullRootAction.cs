using Dynatrace.OpenKit.API;

namespace Dynatrace.OpenKit.Core
{
    public class NullRootAction : NullAction, IRootAction
    {
        public IAction EnterAction(string actionName)
        {
            return new NullAction(this);
        }
    }
}
