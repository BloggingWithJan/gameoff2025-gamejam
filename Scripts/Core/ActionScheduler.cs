using UnityEngine;

namespace GameJam.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        [SerializeField]
        IAction currentAction;

        public void StartAction(IAction action)
        {
            if (currentAction == action)
                return;
            if (currentAction != null)
            {
                currentAction.Cancel();
            }
            currentAction = action;
        }

        public IAction GetCurrentAction()
        {
            return currentAction;
        }

        public void CancelCurrentAction()
        {
            StartAction(null);
        }

        public void CancelIfCurrentActionIs(IAction action)
        {
            if (currentAction == action)
            {
                CancelCurrentAction();
            }
        }
    }
}
