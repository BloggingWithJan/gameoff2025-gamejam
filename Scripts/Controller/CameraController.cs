using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement Settings")] public float moveSpeed = 10f;
        public float scrollSpeed = 100f;
        public float minY = 5f;
        public float maxY = 20f;
        public float tiltAngle = 65f;
        public float rotationSpeed = 50f;

        void Start()
        {
            // Set initial rotation to look down at tiltAngle  
            transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);
        }

        void Update()
        {
            HandleMovement();
            HandleZoom();
            HandleRotation();
        }

        private void HandleMovement()
        {
            Vector3 move = Vector3.zero;
            if (Keyboard.current.wKey.isPressed) move.z += 1f;
            if (Keyboard.current.sKey.isPressed) move.z -= 1f;
            if (Keyboard.current.aKey.isPressed) move.x -= 1f;
            if (Keyboard.current.dKey.isPressed) move.x += 1f;

            // Move in the camera's local XZ plane  
            Vector3 forward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 right = transform.right;
            transform.position += (forward * move.z + right * move.x) * moveSpeed * Time.deltaTime;
        }

        private void HandleZoom()
        {
            if (Mouse.current != null)
            {
                Vector2 scroll = Mouse.current.scroll.ReadValue();
                if (scroll.y != 0f)
                {
                    Vector3 pos = transform.position;
                    pos.y -= scroll.y * scrollSpeed * Time.deltaTime;
                    pos.y = Mathf.Clamp(pos.y, minY, maxY);
                    transform.position = pos;
                }
            }
        }

        private void HandleRotation()
        {
            float rotation = 0f;
            if (Keyboard.current.qKey.isPressed) rotation -= 1f;
            if (Keyboard.current.eKey.isPressed) rotation += 1f;
            transform.Rotate(Vector3.up, rotation * rotationSpeed * Time.deltaTime, Space.World);
            Vector3 euler = transform.eulerAngles;
            euler.x = tiltAngle;
            transform.eulerAngles = euler;
        }
    }
}