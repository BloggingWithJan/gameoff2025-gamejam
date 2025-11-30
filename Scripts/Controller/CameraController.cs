using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    public class CameraController : MonoBehaviour
    {
        [Header("Settings")]
        public float moveSpeed = 50f;
        public float rotationSpeed = 15f;
        public float tiltSpeed = 15f;
        public float zoomSpeed = 150f;
        public float minZoom = 3f;
        public float maxZoom = 60f;
        public InputActionAsset cameraActionsAsset;

        [Header("Camera Bounds")]
        public bool useBounds = true;
        public Vector2 minBounds = new Vector2(-100f, -100f);
        public Vector2 maxBounds = new Vector2(100f, 100f);

        [Tooltip("Draw bounds in Scene view")]
        public bool showBoundsGizmo = true;

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
            transform.position +=
                (forward * input.y + right * input.x) * moveSpeed * Time.deltaTime;

            // Update pivot to maintain relative offset
            _pivotPoint += (forward * input.y + right * input.x) * moveSpeed * Time.deltaTime;

            // Apply bounds
            if (useBounds)
            {
                ClampToBounds();
            }
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

        private void ClampToBounds()
        {
            Vector3 clampedPivot = _pivotPoint;
            clampedPivot.x = Mathf.Clamp(clampedPivot.x, minBounds.x, maxBounds.x);
            clampedPivot.z = Mathf.Clamp(clampedPivot.z, minBounds.y, maxBounds.y);

            // Calculate offset and apply to both pivot and camera
            Vector3 offset = clampedPivot - _pivotPoint;
            _pivotPoint = clampedPivot;
            transform.position += offset;
        }

        private void OnDrawGizmos()
        {
            if (!showBoundsGizmo || !useBounds)
                return;

            Gizmos.color = Color.yellow;
            Vector3 bottomLeft = new Vector3(minBounds.x, 0, minBounds.y);
            Vector3 bottomRight = new Vector3(maxBounds.x, 0, minBounds.y);
            Vector3 topLeft = new Vector3(minBounds.x, 0, maxBounds.y);
            Vector3 topRight = new Vector3(maxBounds.x, 0, maxBounds.y);

            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
    }
}
