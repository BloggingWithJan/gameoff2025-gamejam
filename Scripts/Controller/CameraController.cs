using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 50f;
        public float moveSpeedMultiplierWhenZoomedOut = 2f;

        [Header("Rotation Settings")]
        public float rotationSpeed = 0.1f;
        public float tiltSpeed = 0.1f;

        [Header("Zoom Settings")]
        public float zoomSpeed = 150f;
        public float minZoom = 3f;
        public float maxZoom = 60f;
        public AnimationCurve zoomSpeedCurve = AnimationCurve.Linear(0, 1, 1, 1);

        [Header("Auto-Tilt on Zoom")]
        [Tooltip("X rotation when fully zoomed in (lower = more top-down)")]
        public float minZoomPitch = 30f;

        [Tooltip("X rotation when fully zoomed out (higher = more angled)")]
        public float maxZoomPitch = 60f;
        public bool autoTiltOnZoom = true;

        [Header("Camera Bounds")]
        public bool useBounds = true;
        public Vector2 minBounds = new Vector2(-100f, -100f);
        public Vector2 maxBounds = new Vector2(100f, 100f);
        [Tooltip("Draw bounds in Scene view")]
        public bool showBoundsGizmo = true;

        public InputActionAsset cameraActionsAsset;

        private InputAction _moveAction;
        private InputAction _zoomAction;
        private InputAction _mouseDeltaMove;

        private Vector3 _pivotPoint;
        private float _distanceToPivot;
        private float _pitch; //vertical angle
        private float _yaw; //horizontal angle
        private bool _isManuallyRotating;

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
            {
                HandleMouseOrbit();
                _isManuallyRotating = true;
            }
            else if (Mouse.current.middleButton.wasReleasedThisFrame)
            {
                _isManuallyRotating = false;
            }

            // Auto-tilt based on zoom level
            if (autoTiltOnZoom && !_isManuallyRotating)
            {
                ApplyAutoTilt();
            }

            UpdateCameraPosition();
        }

        private void HandleMovement()
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();
            Vector3 forward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 right = transform.right;

            // Calculate speed multiplier based on zoom distance
            float zoomNormalized = Mathf.InverseLerp(minZoom, maxZoom, _distanceToPivot);
            float speedMultiplier = 1f + (zoomNormalized * (moveSpeedMultiplierWhenZoomedOut - 1f));

            Vector3 moveVector =
                (forward * input.y + right * input.x)
                * moveSpeed
                * speedMultiplier
                * Time.deltaTime;
            transform.position += moveVector;

            // Update pivot to maintain relative offset
            _pivotPoint += moveVector;

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

            // Evaluate zoom speed curve based on current zoom level
            float zoomNormalized = Mathf.InverseLerp(minZoom, maxZoom, _distanceToPivot);
            float curveMultiplier = zoomSpeedCurve.Evaluate(zoomNormalized);

            _distanceToPivot -= zoomAmount * zoomSpeed * curveMultiplier * Time.deltaTime;
            _distanceToPivot = Mathf.Clamp(_distanceToPivot, minZoom, maxZoom);
        }

        private void ApplyAutoTilt()
        {
            // Lerp pitch based on zoom level (closer = lower pitch, farther = higher pitch)
            float zoomNormalized = Mathf.InverseLerp(minZoom, maxZoom, _distanceToPivot);
            float targetPitch = Mathf.Lerp(minZoomPitch, maxZoomPitch, zoomNormalized);
            _pitch = Mathf.Lerp(_pitch, targetPitch, tiltSpeed * Time.deltaTime);
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
