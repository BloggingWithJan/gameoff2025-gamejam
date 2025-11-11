using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace UI.Managers
{
    public class BuildingClickDetector : MonoBehaviour
    {
        [SerializeField] private BuildingInfoPanel buildingInfoPanel;
        [SerializeField] private Button moveButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private BuildBuildingPlacerManager _buildingPlacerManager;
        
        private GameObject currentBuilding;
        

        void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
                    return;
                }

                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (!hit.collider.CompareTag("Building"))
                    {
                        buildingInfoPanel.ClosePanel();
                        return;
                    }
                    
                    GameObject clickedObject = hit.collider.gameObject;

                    // toggle if same building clicked
                    if (buildingInfoPanel.gameObject.activeSelf && currentBuilding == clickedObject)
                    {
                        buildingInfoPanel.ClosePanel();
                        currentBuilding = null;
                        return;
                    }

                    BuildingData buildingData = clickedObject.GetComponent<BuildingData>();
                    currentBuilding = clickedObject;
                    buildingInfoPanel.SetData(buildingData);
                    buildingInfoPanel.ShowPanel();
                }
            }
        }
    }
}