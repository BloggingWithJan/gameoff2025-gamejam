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
        [SerializeField] private GameObject buildingInfoPanel;
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
            buildingInfoPanel.SetActive(false);
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
                    if (buildingInfoPanel.activeSelf && currentBuilding == clicked)
                    {
                        ClosePanel();
                        return;
                    }

                    BuildingData data = clicked.GetComponent<BuildingData>();
                    if (data == null) return;

                    currentBuilding = clicked;
                    followTarget = clicked.transform;
                    SetPanelData(data);
                    ShowPanel();
                }
            }

            // Follow target if panel open
            if (buildingInfoPanel.activeSelf && followTarget != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(followTarget.position + offset);
                panelRect.position = screenPos;
            }
        }

        void SetPanelData(BuildingData building)
        {
            buildingNameText.text = building.buildingName;
            descriptionText.text = building.description;
        }

        void ShowPanel()
        {
            buildingInfoPanel.SetActive(true);
        }

        void ClosePanel()
        {
            buildingInfoPanel.SetActive(false);
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
