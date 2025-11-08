using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    public class CameraController : MonoBehaviour
    {
        [Header("Settings")] public float moveSpeed = 30f;
        public float mousePanSpeed = 5f;
        public float tiltAngle = 65f;
        public float rotationSpeed = 75f;
        public float zoomSpeed = 150f;
        public float minZoom = 3f;
        public float maxZoom = 30f;
        public InputActionAsset cameraActionsAsset;

        private InputAction _moveAction;
        private InputAction _zoomAction;
        private InputAction _rotateAction;
        private InputAction _mouseDeltaMove;
        private float _targetY; //for smooth scrolling

        private void OnEnable()
        {
            var map = cameraActionsAsset.FindActionMap("Camera");

            _moveAction = map.FindAction("Move");
            _moveAction.Enable();

            _zoomAction = map.FindAction("Zoom");
            _zoomAction.Enable();

            _rotateAction = map.FindAction("Rotate");
            _rotateAction.Enable();

            _mouseDeltaMove = map.FindAction("MouseDeltaMove");
            _mouseDeltaMove.Enable();
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _zoomAction.Disable();
            _rotateAction.Disable();
        }


        private void Start()
        {
            transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);
            _targetY = transform.position.y;
        }

        private void Update()
        {
            HandleMovement();
            HandleZoom();
            HandleRotation();
            HandleMousePan();
        }

        private void HandleMovement()
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();
            Vector3 forward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 right = transform.right;
            transform.position += (forward * input.y + right * input.x) * moveSpeed * Time.deltaTime;
        }

        private void HandleZoom()
        {
            Vector2 scroll = _zoomAction.ReadValue<Vector2>();
            float zoomAmount = scroll.y;

            _targetY -= zoomAmount * zoomSpeed * Time.deltaTime;
            _targetY = Mathf.Clamp(_targetY, minZoom, maxZoom);

            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, _targetY, 5f * Time.deltaTime); //smooth transition
            transform.position = pos;
        }

        private void HandleRotation()
        {
            float rotationInput = _rotateAction.ReadValue<float>();
            transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime, Space.World);

            Vector3 euler = transform.eulerAngles;
            euler.x = tiltAngle;
            transform.eulerAngles = euler;
        }


        private void HandleMousePan()
        {
            // Only pan if middle mouse is held
            if (Mouse.current.middleButton.isPressed)
            {
                Vector2 delta = _mouseDeltaMove.ReadValue<Vector2>();

                Vector3 right = transform.right;
                Vector3 forward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;

                // Invert delta to match drag motion
                transform.position += (right * -delta.x + forward * -delta.y) * mousePanSpeed * Time.deltaTime;
            }
        }
    }
}