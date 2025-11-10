using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace UI.Managers
{
    public class BuildingClickDetector : MonoBehaviour
    {
        [SerializeField] private BuildingInfoPanel buildingInfoPanel;
        private Camera mainCam;
        private GameObject currentBuilding;

        void Awake()
        {
            mainCam = Camera.main;
        }

        void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (!hit.collider.CompareTag("Building")) {
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

                    BuildingData building = clickedObject.GetComponent<BuildingData>();
                    if (building == null)
                        return;

                    currentBuilding = clickedObject;
                    buildingInfoPanel.SetData(building);
                }
            }
        }
    }
}