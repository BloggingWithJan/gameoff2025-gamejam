using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Managers
{
    public class InputManager : MonoBehaviour
    {
        public InputActionAsset uiControlsAsset;
        public GameObject buildingPanel;

        private InputAction _bAction;

        private void OnEnable()
        {
            var map = uiControlsAsset.FindActionMap("UI");
            _bAction = map.FindAction("PressB");
            _bAction.performed += OnBPressed;
            _bAction.Enable();
        }

        private void OnDisable()
        {
            _bAction.performed -= OnBPressed;
            _bAction.Disable();
        }

        private void OnBPressed(InputAction.CallbackContext context)
        {
            if (buildingPanel != null)
            {
                buildingPanel.SetActive(!buildingPanel.activeSelf);
            }
        }
    }
}