using Core;
using Data;
using GameJam.Core;
using TMPro;
using UI.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Controller.UI
{
    public class BuildingInfoPanel : MonoBehaviour
    {
        public static BuildingInfoPanel Instance { get; private set; }

        [Header("UI References")]
        [SerializeField]
        private TMP_Text buildingNameText;

        [SerializeField]
        private TMP_Text descriptionText;

        [SerializeField]
        private Button moveButton;

        [SerializeField]
        private Button deleteButton;

        [Header("Controller Reference")]
        [SerializeField]
        private BuildingPlacementController buildingPlacementController;

        [Header("Follow Settings")]
        [SerializeField]
        private Vector3 offset = new Vector3(0, 1f, 0);

        private Transform followTarget;
        private RectTransform rect;
        private GameObject currentBuilding;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            rect = GetComponent<RectTransform>();
            gameObject.SetActive(false);

            // Add button listeners
            if (moveButton != null)
                moveButton.onClick.AddListener(OnMoveClicked);
            if (deleteButton != null)
                deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        private void Update()
        {
            if (IsShown() && followTarget != null)
            {
                UpdatePosition();
            }
            else
            {
                HidePanel();
            }
        }

        private void UpdatePosition()
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(followTarget.position + offset);
            rect.position = screenPos;
        }

        public void ShowPanel(BaseBuilding building)
        {
            if (building == null)
                return;

            currentBuilding = building.gameObject;
            followTarget = building.transform;

            buildingNameText.text = building.buildingName;
            descriptionText.text = building.description;

            moveButton.interactable = building.isMovable;
            deleteButton.interactable = building.isDismantable;

            UpdatePosition();

            gameObject.SetActive(true);
        }

        public void HidePanel()
        {
            currentBuilding = null;
            followTarget = null;
            gameObject.SetActive(false);
        }

        public bool IsShown() => gameObject.activeSelf;

        private void OnMoveClicked()
        {
            if (currentBuilding != null && buildingPlacementController != null)
            {
                buildingPlacementController.RepositionBuilding(currentBuilding);
            }

            HidePanel();
        }

        private void OnDeleteClicked()
        {
            if (currentBuilding != null && buildingPlacementController != null)
            {
                buildingPlacementController.DeleteBuilding(currentBuilding);
            }

            HidePanel();
        }
    }
}
