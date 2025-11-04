using GameJam.Worker;
using UnityEngine;

namespace GameJam.UI
{
    public class StateInfo : MonoBehaviour
    {
        [SerializeField]
        GameObject workerUnit;

        [SerializeField]
        GameObject worldSpaceCanvas;

        void LateUpdate()
        {
            WorkerUnit worker = workerUnit.GetComponent<WorkerUnit>();
            if (
                worker != null
                && worker.currentState == WorkerUnit.WorkerState.SearchingForResource
            )
            {
                worldSpaceCanvas.SetActive(true);
            }
            else
            {
                worldSpaceCanvas.SetActive(false);
            }
        }
    }
}
