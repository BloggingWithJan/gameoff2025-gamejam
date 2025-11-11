using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Managers
{
    public class BuildBuildingPanelManager : MonoBehaviour
    {
        public GameObject buildingPanel;

        public BuildBuildingPlacerManager placerManager;
        public BuildBuildingTooltipManager tooltipManager;

        [Header("Buttons")] public Button quarryButton;
        public Button baseButton;
        public Button towerButton;
        public Button rockButton;
        public Button treeButton;
        public Button lumberjackLodgeButton;

        [Header("Prefabs")] public GameObject quarryPrefab;
        public GameObject basePrefab;
        public GameObject towerPrefab;
        public GameObject rockPrefab;
        public GameObject treePrefab;
        public GameObject lumberjackLodgePrefab;

        private void Awake()
        {
            // Setup Quarry
            quarryButton.onClick.AddListener(() => placerManager.StartPlacement(quarryPrefab));
            if (quarryPrefab.TryGetComponent<BuildingData>(out BuildingData quarryData))
            {
                AddTooltipEvents(quarryButton, quarryData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }
            
            // Setup Lumberjack Lodge
            lumberjackLodgeButton.onClick.AddListener(() => placerManager.StartPlacement(lumberjackLodgePrefab));
            if (lumberjackLodgePrefab.TryGetComponent<BuildingData>(out BuildingData lumberjackLodgeData))
            {
                AddTooltipEvents(lumberjackLodgeButton, lumberjackLodgeData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }

            // Setup Base
            baseButton.onClick.AddListener(() => placerManager.StartPlacement(basePrefab));
            if (basePrefab.TryGetComponent<BuildingData>(out BuildingData baseData))
            {
                AddTooltipEvents(baseButton, baseData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }
            
            // Setup Tower
            towerButton.onClick.AddListener(() => placerManager.StartPlacement(towerPrefab));
            if (towerPrefab.TryGetComponent<BuildingData>(out BuildingData towerData))
            {
                AddTooltipEvents(towerButton, towerData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }

            // Setup Rock
            rockButton.onClick.AddListener(() => placerManager.StartPlacement(rockPrefab));
            if (rockPrefab.TryGetComponent<BuildingData>(out BuildingData rockData))
            {
                AddTooltipEvents(rockButton, rockData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }

            // Setup Tree
            treeButton.onClick.AddListener(() => placerManager.StartPlacement(treePrefab));
            if (treePrefab.TryGetComponent<BuildingData>(out BuildingData treeData))
            {
                AddTooltipEvents(treeButton, treeData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
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
                tooltipManager.SetTooltip(data.buildingName, data.description, data.costs);
                tooltipManager.ShowTooltip();
            });
            trigger.triggers.Add(entryEnter);

            // PointerExit
            EventTrigger.Entry entryExit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            entryExit.callback.AddListener((_) => tooltipManager.HideTooltip());
            trigger.triggers.Add(entryExit);
        }

        public void TogglePanel()
        {
            if (buildingPanel != null)
                buildingPanel.SetActive(!buildingPanel.activeSelf);
        }
    }
}