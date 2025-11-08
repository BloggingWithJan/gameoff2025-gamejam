using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Managers
{
    public class BuildingPanelManager : MonoBehaviour
    {
        public GameObject buildingPanel;

        public BuildingPlacerManager placerManager;
        public BuildBuildingTooltipManager tooltipManager;

        [Header("Buttons")] public Button quarryButton;
        public Button baseButton;
        public Button rockButton;
        public Button treeButton;

        private void Awake()
        {
            //TODO replace
            Sprite defaultSprite = Resources.Load<Sprite>("Sprites/defaultsprite");

            // Setup Quarry
            quarryButton.onClick.AddListener(() => placerManager.StartPlacement(placerManager.quarryPrefab));
            AddTooltipEvents(quarryButton, "Quarry", "Mines rocks like a champ.", "Cost: 100 gold", defaultSprite);

            // Setup Base
            baseButton.onClick.AddListener(() => placerManager.StartPlacement(placerManager.basePrefab));
            AddTooltipEvents(baseButton, "Base", "Your main headquarters.", "Cost: 500 gold", defaultSprite);

            // Setup Rock
            rockButton.onClick.AddListener(() => placerManager.StartPlacement(placerManager.rockPrefab));
            AddTooltipEvents(rockButton, "Rock", "Just a big rock.", "Cost: 10 gold", defaultSprite);

            // Setup Tree
            treeButton.onClick.AddListener(() => placerManager.StartPlacement(placerManager.treePrefab));
            AddTooltipEvents(treeButton, "Tree", "Provides wood over time.", "Cost: 20 gold", defaultSprite);
        }

        private void AddTooltipEvents(Button button, string name, string description, string cost, Sprite icon)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            // PointerEnter
            EventTrigger.Entry entryEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            entryEnter.callback.AddListener((data) => ShowTooltip(name, description, cost, icon));
            trigger.triggers.Add(entryEnter);

            // PointerExit
            EventTrigger.Entry entryExit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            entryExit.callback.AddListener((data) => HideTooltip());
            trigger.triggers.Add(entryExit);
        }

        private void ShowTooltip(string buildingName, string description, string cost, Sprite icon)
        {
            tooltipManager.tooltipPanel.SetActive(true);
            tooltipManager.buildingName.text = buildingName;
            tooltipManager.buildingDescription.text = description;
            tooltipManager.buildingCostText.text = cost;
            tooltipManager.buildingIcon.sprite = icon;
        }

        private void HideTooltip()
        {
            tooltipManager.tooltipPanel.SetActive(false);
        }

        public void TogglePanel()
        {
            if (buildingPanel != null)
                buildingPanel.SetActive(!buildingPanel.activeSelf);
        }
    }
}