using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    public class CameraController : MonoBehaviour
    {
        [Header("Settings")] public float moveSpeed = 10f;
        public float tiltAngle = 65f;
        public float rotationSpeed = 75f;
        public float zoomSpeed = 75f;
        public InputActionAsset cameraActionsAsset;

        private InputAction _moveAction;
        private InputAction _zoomAction;
        private InputAction _rotateAction;
        private float _targetY; //for smooth scrolling
        private const float MinZoom = 5f;
        private const float MaxZoom = 20f;

        private void OnEnable()
        {
            var map = cameraActionsAsset.FindActionMap("Camera");

            _moveAction = map.FindAction("Move");
            _moveAction.Enable();

            _zoomAction = map.FindAction("Zoom");
            _zoomAction.Enable();

            _rotateAction = map.FindAction("Rotate");
            _rotateAction.Enable();
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
            _targetY = Mathf.Clamp(_targetY, MinZoom, MaxZoom);

            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, _targetY, 10f * Time.deltaTime); //smooth transition
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
    }
}