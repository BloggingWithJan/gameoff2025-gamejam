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

            GameObject ft = Instantiate(floatingTextPrefab, uiCanvas.transform);
            ft.transform.position = mousePos;

            ft.GetComponent<FloatingText>().SetText(message, color);
        }
    }
}