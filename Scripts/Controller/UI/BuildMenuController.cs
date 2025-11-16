using Data;
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

        [FormerlySerializedAs("placerManager")] public BuildingPlacementController placementController;
        [FormerlySerializedAs("tooltipManager")] public BuildingTooltipController tooltipController;

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
                    placementController.StartPlacement(entry.prefab));

                if (entry.prefab.TryGetComponent<BuildingData>(out var data))
                {
                    AddTooltipEvents(entry.button, data);
                }
                else
                {
                    Debug.LogError($"BuildingData missing on prefab {entry.prefab.name}");
                }
            }
        }


        private void AddTooltipEvents(Button button, BuildingData data)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            // PointerEnter
            EventTrigger.Entry entryEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entryEnter.callback.AddListener((_) =>
            {
                tooltipController.SetTooltip(data.buildingName, data.description, data.costs);
                tooltipController.ShowTooltip();
            });
            trigger.triggers.Add(entryEnter);

            // PointerExit
            EventTrigger.Entry entryExit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            entryExit.callback.AddListener((_) => tooltipController.HideTooltip());
            trigger.triggers.Add(entryExit);
        }

        public void TogglePanel()
        {
            if (buildingPanel != null)
                buildingPanel.SetActive(!buildingPanel.activeSelf);
        }
    }
}