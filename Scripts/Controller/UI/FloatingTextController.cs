using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller.UI
{
    public class FloatingTextController : MonoBehaviour
    {
        public static FloatingTextController Instance { get; private set; }

        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private GameObject floatingTextPrefab;

        [Header("Floating Text Settings")]
        [SerializeField] private float verticalOffset = 20f; // distance between stacked texts
        [SerializeField] private float randomOffsetX = 10f;  // optional small horizontal spread

        private int _activeTextCount = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void ShowFloatingText(string message, Color color)
        {
            if (Mouse.current == null || floatingTextPrefab == null || uiCanvas == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Offset each new text vertically to avoid overlap
            Vector2 spawnPos = mousePos;
            spawnPos.y += _activeTextCount * verticalOffset;
            spawnPos.x += Random.Range(-randomOffsetX, randomOffsetX); // small horizontal randomness

            GameObject ft = Instantiate(floatingTextPrefab, uiCanvas.transform);
            ft.transform.position = spawnPos;

            ft.GetComponent<FloatingText>().SetText(message, color);

            _activeTextCount++;

            // Reset counter after a short delay (so next batch of texts starts at mouse again)
            // Optional: you could also reset in FloatingText itself when it disappears
            Destroy(ft, 2f); // ensures text is destroyed after lifetime
            Invoke(nameof(ResetActiveCount), 0.1f);
        }

        private void ResetActiveCount()
        {
            _activeTextCount = 0;
        }
    }
}
