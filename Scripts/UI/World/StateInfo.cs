using GameJam.Worker;
using UnityEngine;

namespace GameJam.UI
{
    public class StateInfo : MonoBehaviour
    {
        [SerializeField]
        GameObject gathererUnit;

        [SerializeField]
        GameObject worldSpaceCanvas;

        void LateUpdate()
        {
            Gatherer gatherer = gathererUnit.GetComponent<Gatherer>();
            if (
                gatherer != null
                && gatherer.currentState == Gatherer.GathererState.SearchingForResource
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
