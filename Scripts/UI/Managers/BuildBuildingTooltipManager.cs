using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Managers
{
    public class BuildBuildingTooltipManager : MonoBehaviour
    {
        public GameObject tooltipPanel;
        public TMP_Text buildingName;
        public TMP_Text buildingDescription;
        public TMP_Text buildingCostText;
        public Image buildingIcon;

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

        public void SetTooltip(string name, string description, string cost)
        {
            buildingName.text = name;
            buildingDescription.text = description;
            buildingCostText.text = cost;
        }

        public void ShowTooltip(string name, string description, string cost)
        {
            SetTooltip(name, description, cost);
            tooltipPanel.SetActive(true);
        }

        public void HideTooltip()
        {
            tooltipPanel.SetActive(false);
        }
    }
}