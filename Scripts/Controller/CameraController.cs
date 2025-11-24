using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    public class CameraController : MonoBehaviour
    {
        [Header("Settings")] public float moveSpeed = 50f;
        public float rotationSpeed = 0.1f;
        public float tiltSpeed = 0.1f;
        public float zoomSpeed = 150f;
        public float minZoom = 3f;
        public float maxZoom = 60f;
        public InputActionAsset cameraActionsAsset;

        private InputAction _moveAction;
        private InputAction _zoomAction;
        private InputAction _mouseDeltaMove;

        private Vector3 _pivotPoint;
        private float _distanceToPivot;
        private float _pitch; //vertical angle
        private float _yaw; //horizontal angle

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

        private void Start()
        {
            // Initialize pivot point to current forward direction
            _pivotPoint = transform.position + transform.forward * 10f;
            _distanceToPivot = Vector3.Distance(transform.position, _pivotPoint);

            Vector3 angles = transform.eulerAngles;
            _yaw = angles.y;
            _pitch = angles.x;
        }

        private void Update()
        {
            HandleMovement();
            HandleZoom();

            if (Mouse.current.middleButton.isPressed)
                HandleMouseOrbit();
        }

        private void HandleMovement()
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();
            Vector3 forward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 right = transform.right;
            transform.position += (forward * input.y + right * input.x) * moveSpeed * Time.deltaTime;

            // Update pivot to maintain relative offset
            _pivotPoint += (forward * input.y + right * input.x) * moveSpeed * Time.deltaTime;
        }

        private void HandleZoom()
        {
            Vector2 scroll = _zoomAction.ReadValue<Vector2>();
            float zoomAmount = scroll.y;

            _distanceToPivot -= zoomAmount * zoomSpeed * Time.deltaTime;
            _distanceToPivot = Mathf.Clamp(_distanceToPivot, minZoom, maxZoom);

            UpdateCameraPosition();
        }

        /**
         * Rotation and Tilting
         */
        private void HandleMouseOrbit()
        {
            Vector2 delta = _mouseDeltaMove.ReadValue<Vector2>();

            // Normalize for different framerates AND different mouse hardware
            float scale = 0.015f; // tweak this once until Game View == Build speeds

            _yaw += delta.x * rotationSpeed * scale;
            _pitch -= delta.y * tiltSpeed * scale;

            // Clamp vertical angle to avoid flipping
            _pitch = Mathf.Clamp(_pitch, 10f, 85f);

            UpdateCameraPosition();
        }

        private void UpdateCameraPosition()
        {
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.position = _pivotPoint - rotation * Vector3.forward * _distanceToPivot;
            transform.LookAt(_pivotPoint);
        }
    }
}