using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Managers
{
    public class FloatingTextManager : MonoBehaviour
    {
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private GameObject floatingTextPrefab;

        public void ShowFloatingText(string message)
        {
            if (Mouse.current == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();

            GameObject ft = Instantiate(floatingTextPrefab, uiCanvas.transform);
            ft.transform.position = mousePos;

            ft.GetComponent<FloatingText>().SetText(message);
        }
    }
}