using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public InputActionAsset uiControlsAsset; // assign your UIControls input actions here
    public GameObject buildingPanel; // assign your panel here

    private InputAction _bAction;

    private void OnEnable()
    {
        var map = uiControlsAsset.FindActionMap("UI"); // match your action map name
        _bAction = map.FindAction("PressB"); // match your action name
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
            buildingPanel.SetActive(!buildingPanel.activeSelf); // toggle panel
        }
    }
}