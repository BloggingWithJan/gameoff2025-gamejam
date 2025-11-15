using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller.UI
{
    public class TooltipController : MonoBehaviour
    {
        public static TooltipController Instance;

        [SerializeField] private RectTransform tooltipRoot;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private Vector2 offset = new Vector2(0, -60f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            tooltipRoot.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (tooltipRoot.gameObject.activeSelf)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Vector2 clampedPos = ClampToScreen(mousePos + offset);
                tooltipRoot.position = clampedPos;
            }
        }

        public void Show(string text)
        {
            tooltipText.text = text;
            tooltipRoot.gameObject.SetActive(true);
        }

        public void Hide()
        {
            tooltipRoot.gameObject.SetActive(false);
        }

        private Vector2 ClampToScreen(Vector2 position, float horizontalPadding = 10f, float verticalPadding = 10f)
        {
            // Get tooltip corners in screen space
            Vector3[] corners = new Vector3[4];
            tooltipRoot.GetWorldCorners(corners);

            float width = corners[2].x - corners[0].x;  // right - left
            float height = corners[2].y - corners[0].y; // top - bottom

            // Clamp X with horizontal padding
            float x = Mathf.Clamp(
                position.x,
                horizontalPadding + width * tooltipRoot.pivot.x,
                Screen.width - horizontalPadding - width * (1 - tooltipRoot.pivot.x)
            );

            // Clamp Y with vertical padding
            float y = Mathf.Clamp(
                position.y,
                verticalPadding + height * tooltipRoot.pivot.y,
                Screen.height - verticalPadding - height * (1 - tooltipRoot.pivot.y)
            );

            return new Vector2(x, y);
        }
    }
}