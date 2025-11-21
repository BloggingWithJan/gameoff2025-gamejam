using System.Collections.Generic;
using System.Resources;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ResourceManager = GameJam.Resource.ResourceManager;

namespace UI.Managers
{
    public class BuildingTooltipController : MonoBehaviour
    {
        public GameObject tooltipPanel;
        public TMP_Text buildingName;
        public TMP_Text buildingDescription;

        [Header("Resource Panels")]
        public List<ResourceUIPanel> resourcePanels;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = tooltipPanel.GetComponent<RectTransform>();

            tooltipPanel.SetActive(false);
        }

        private void Update()
        {
            if (!tooltipPanel.activeSelf || UnityEngine.InputSystem.Mouse.current == null)
                return;

            Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();

            RectTransform rt = rectTransform;
            float width = rt.rect.width * rt.lossyScale.x;
            float height = rt.rect.height * rt.lossyScale.y;

            // Offsets from the mouse
            float offsetX = -20f; // to the left
            float offsetY = -20f; // above the mouse

            // Initial tooltip position
            float posX = mousePos.x + offsetX - width;
            float posY = mousePos.y + offsetY;

            // Clamp to screen bounds
            posX = Mathf.Clamp(posX, 0f, Screen.width - width);
            posY = Mathf.Clamp(posY, 0f, Screen.height - height);

            rectTransform.position = new Vector3(posX, posY, 0f);
        }

        public void SetTooltip(string name, string description, List<ResourceCost> costs)
        {
            buildingName.text = name;
            buildingDescription.text = description;

            //Hide all resource panels initially
            foreach (var panel in resourcePanels)
                panel.panel.SetActive(false);

            //Show panels that exist in the costs list
            foreach (var cost in costs)
            {
                var uiPanel = resourcePanels.Find(p => p.resource == cost.resource);
                if (uiPanel != null)
                {
                    uiPanel.amountText.text = cost.amount.ToString();
                    uiPanel.amountText.color = ResourceManager.Instance.HasSufficientResources(cost)
                        ? Color.white
                        : Color.red;
                    uiPanel.panel.SetActive(true);
                }
            }
        }

        public void ShowTooltip()
        {
            tooltipPanel.SetActive(true);
        }

        public void HideTooltip()
        {
            tooltipPanel.SetActive(false);
        }
    }
}
