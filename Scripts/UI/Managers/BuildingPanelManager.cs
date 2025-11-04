using UnityEngine;

namespace UI
{
    public class BuildingPanelManager : MonoBehaviour
    {
        public GameObject buildingPanel;

        public void TogglePanel()
        {
            if (buildingPanel != null)
            {
                buildingPanel.SetActive(!buildingPanel.activeSelf);
            }
        }
    }
}
