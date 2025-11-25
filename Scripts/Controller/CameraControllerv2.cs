using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    public class CameraControllerv2 : MonoBehaviour
    {
        [Header("Settings")]
        public InputActionAsset cameraActionsAsset;

        private InputAction _moveAction;
        private InputAction _zoomAction;
        private InputAction _mouseDeltaMove;

        private void OnEnable()
        {
            var map = cameraActionsAsset.FindActionMap("Camera");

            _moveAction = map.FindAction("Move");
            _moveAction.Enable();

            _zoomAction = map.FindAction("Zoom");
            _zoomAction.Enable();

            _mouseDeltaMove = map.FindAction("MouseDeltaMove");
            _mouseDeltaMove.Enable();
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _zoomAction.Disable();
            _mouseDeltaMove.Disable();
        }
    }
}
