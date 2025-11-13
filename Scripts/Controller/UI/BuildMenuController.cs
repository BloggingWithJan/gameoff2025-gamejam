using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Managers
{
    public class BuildMenuController : MonoBehaviour
    {
        public GameObject buildingPanel;

        [FormerlySerializedAs("placerManager")] public BuildingPlacementController placementController;
        [FormerlySerializedAs("tooltipManager")] public BuildingTooltipController tooltipController;

        [Header("Buttons")] public Button quarryButton;
        public Button baseButton;
        public Button towerButton;
        public Button rockButton;
        public Button treeButton;
        public Button lumberjackLodgeButton;
        public Button foodLodgeButton;

        [Header("Prefabs")] public GameObject quarryPrefab;
        public GameObject basePrefab;
        public GameObject towerPrefab;
        public GameObject rockPrefab;
        public GameObject treePrefab;
        public GameObject lumberjackLodgePrefab;
        public GameObject foodLodgePrefab;

        private void Awake()
        {
            // Setup Quarry
            quarryButton.onClick.AddListener(() => placementController.StartPlacement(quarryPrefab));
            if (quarryPrefab.TryGetComponent<BuildingData>(out BuildingData quarryData))
            {
                AddTooltipEvents(quarryButton, quarryData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }
            
            // Setup Lumberjack Lodge
            lumberjackLodgeButton.onClick.AddListener(() => placementController.StartPlacement(lumberjackLodgePrefab));
            if (lumberjackLodgePrefab.TryGetComponent<BuildingData>(out BuildingData lumberjackLodgeData))
            {
                AddTooltipEvents(lumberjackLodgeButton, lumberjackLodgeData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }
            
            // Setup Food Lodge
            foodLodgeButton.onClick.AddListener(() => placementController.StartPlacement(foodLodgePrefab));
            if (foodLodgePrefab.TryGetComponent<BuildingData>(out BuildingData foodLodgeData))
            {
                AddTooltipEvents(foodLodgeButton, foodLodgeData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }

            // Setup Base
            baseButton.onClick.AddListener(() => placementController.StartPlacement(basePrefab));
            if (basePrefab.TryGetComponent<BuildingData>(out BuildingData baseData))
            {
                AddTooltipEvents(baseButton, baseData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }
            
            // Setup Tower
            towerButton.onClick.AddListener(() => placementController.StartPlacement(towerPrefab));
            if (towerPrefab.TryGetComponent<BuildingData>(out BuildingData towerData))
            {
                AddTooltipEvents(towerButton, towerData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }

            // Setup Rock
            rockButton.onClick.AddListener(() => placementController.StartPlacement(rockPrefab));
            if (rockPrefab.TryGetComponent<BuildingData>(out BuildingData rockData))
            {
                AddTooltipEvents(rockButton, rockData);
            }
            else
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }

            // Setup Tree
            treeButton.onClick.AddListener(() => placementController.StartPlacement(treePrefab));
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