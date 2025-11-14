using TMPro;
using UnityEngine;

namespace UI
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float floatSpeed = 50f; // pixels per second
        [SerializeField] private float fadeDuration = 1f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private float elapsed;

        public void SetText(string message, Color color)
        {
            text.text = message;
            text.color = color;
            elapsed = 0f;
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Update()
        {
            elapsed += Time.deltaTime;

            // Move text upwards
            rectTransform.anchoredPosition += Vector2.up * floatSpeed * Time.deltaTime;

            // Fade out
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            // Destroy after fade
            if (elapsed >= fadeDuration)
            {
                Destroy(gameObject);
            }
        }
    }
}