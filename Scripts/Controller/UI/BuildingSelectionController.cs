using TMPro;
using UI.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Controller.UI
{
    public class BuildingClickDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BuildingInfoPanel buildingInfoPanel;
        [SerializeField] private TMP_Text buildingNameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button moveButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private BuildingPlacementController buildingPlacementController;

        private GameObject currentBuilding;
        private RectTransform panelRect;
        private Transform followTarget;
        private Vector3 offset = new Vector3(0, 2f, 0);

        void Awake()
        {
            moveButton.onClick.AddListener(OnMoveClicked);
            deleteButton.onClick.AddListener(OnDeleteClicked);
            panelRect = buildingInfoPanel.GetComponent<RectTransform>();
            buildingInfoPanel.HidePanel();
        }

        void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (!hit.collider.CompareTag("Building"))
                    {
                        ClosePanel();
                        return;
                    }

                    GameObject clicked = hit.collider.gameObject;

                    // Toggle off if same building clicked again
                    if (buildingInfoPanel.IsShown() && currentBuilding == clicked)
                    {
                        ClosePanel();
                        return;
                    }

                    BuildingData data = clicked.GetComponent<BuildingData>();
                    if (data == null) return;

                    currentBuilding = clicked;
                    followTarget = clicked.transform;
                    ShowPanel(data);
                }
            }

            // Follow target if panel open
            if (buildingInfoPanel.IsShown() && followTarget != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(followTarget.position + offset);
                panelRect.position = screenPos;
            }
        }

        void ShowPanel(BuildingData data)
        {
            buildingInfoPanel.ShowPanel(data);
        }

        void ClosePanel()
        {
            buildingInfoPanel.HidePanel();
            currentBuilding = null;
            followTarget = null;
        }

        void OnMoveClicked()
        {
            if (currentBuilding != null)
                buildingPlacementController.RepositionBuilding(currentBuilding);
            ClosePanel();
        }

        void OnDeleteClicked()
        {
            if (currentBuilding != null)
                buildingPlacementController.DeleteBuilding(currentBuilding);
            ClosePanel();
        }
    }
}
