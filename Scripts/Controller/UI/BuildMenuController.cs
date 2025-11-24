using Core;
using Data;
using GameJam.Core;
using UI.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Controller.UI
{
    public class BuildMenuController : MonoBehaviour
    {
        public GameObject buildingPanel;

        [FormerlySerializedAs("placerManager")]
        public BuildingPlacementController placementController;

        [FormerlySerializedAs("tooltipManager")]
        public PurchaseTooltipController tooltipController;

        public BuildEntry[] buildEntries;

        [System.Serializable]
        public struct BuildEntry
        {
            public Button button;
            public GameObject prefab;
        }

        private void Awake()
        {
            foreach (var entry in buildEntries)
            {
                if (entry.button == null || entry.prefab == null)
                {
                    Debug.LogError("Build entry is missing a button or prefab.");
                    continue;
                }

                entry.button.onClick.AddListener(() =>
                    placementController.StartPlacement(entry.prefab)
                );

                if (entry.prefab.TryGetComponent<BaseBuilding>(out var data))
                {
                    tooltipController.AddTooltipEvents(entry.button, data.buildingName, data.description, data.costs);
                }
                else
                {
                    Debug.LogError($"BaseBuilding missing on prefab {entry.prefab.name}");
                }
            }
        }

        public void TogglePanel()
        {
            if (buildingPanel != null)
                buildingPanel.SetActive(!buildingPanel.activeSelf);
        }
    }
}