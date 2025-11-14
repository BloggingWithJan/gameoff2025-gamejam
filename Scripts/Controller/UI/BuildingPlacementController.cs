using System.Collections.Generic;
using Controller.UI;
using Data;
using Resource;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace UI.Managers
{
    public class BuildingPlacementController : MonoBehaviour
    {
        public LayerMask groundMask;
        public GameObject buildPlacingControls;
        public GameObject buildingsParentGameObject;
        
        private GameObject _currentPrefab;
        private GameObject _previewInstance;
        private bool _isPlacing;

        //if a building gets moved
        private GameObject _currentBuilding;
        private bool _ismoving;

        private Renderer[] _previewRenderers;
        private LineRenderer _previewOutline;
        private readonly Dictionary<Collider, LineRenderer> _blockedOutlines = new();
        private Vector2? _lastMousePos;
        private float _rotationSpeed = 0.3f;
        private float _maxRaycastDistance = 500f;

        private bool TryGetGroundHit(out RaycastHit hit)
        {
            if (Camera.main == null)
            {
                hit = default;
                return false;
            }

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            return Physics.Raycast(ray, out hit, _maxRaycastDistance, groundMask);
        }

        void Update()
        {
            if (!_isPlacing) return;

            if (Mouse.current == null || !TryGetGroundHit(out RaycastHit hit)) {
                Debug.LogError("No ground hit");
                return;
            }
                

            _previewInstance.transform.position = hit.point;

            bool isOverUI = IsPointerOverUI();
            var (isValid, blockers) = IsValidPlacement(hit.point);

            UpdateVisuals(isOverUI, isValid, blockers);

            HandleInput(hit, isOverUI, isValid);
        }

        private void UpdateVisuals(bool isOverUI, bool isValid, List<Collider> blockers)
        {
            Color previewColor = (isOverUI || !isValid) ? Color.red : Color.green;
            SetPreviewColor(previewColor);

            if (_previewInstance.TryGetComponent<BoxCollider>(out var boxCollider))
            {
                DrawOutlineForBoxCollider(boxCollider, _previewOutline, previewColor, 0.02f);
            }

            UpdateBlockingOutlines(blockers);
        }

        private void HandleInput(RaycastHit hit, bool overUI, bool valid)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelPlacement();
                return;
            }

            if (Keyboard.current.rKey.isPressed)
            {
                RotatePreview();
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (overUI)
                {
                    FloatingTextController.Instance.ShowFloatingText("Cannot place over UI!", Color.red);
                    return;
                }

                if (!valid)
                {
                    FloatingTextController.Instance.ShowFloatingText("Placement blocked!", Color.red);
                    return;
                }

                PlaceBuilding(hit.point);
            }
        }

        private (bool valid, List<Collider> blockers) IsValidPlacement(Vector3 position)
        {
            List<Collider> blocking = new();
            if (!_previewInstance) return (true, blocking);
            BoxCollider box = _previewInstance.GetComponent<BoxCollider>();
            if (!box) return (true, blocking);
            box.enabled = false;
            Vector3 worldCenter = position + _previewInstance.transform.rotation * box.center;
            Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, _previewInstance.transform.lossyScale);
            Collider[] overlaps = Physics.OverlapBox(worldCenter, halfExtents, _previewInstance.transform.rotation);
            box.enabled = true;
            foreach (Collider c in overlaps)
            {
                if (c.isTrigger || c.CompareTag("Ground") || c.gameObject == _previewInstance) continue;
                blocking.Add(c);
            }

            return (blocking.Count == 0, blocking);
        }

        private void UpdateBlockingOutlines(List<Collider> newBlockers)
        {
            HashSet<Collider> newSet = new(newBlockers);
            foreach (var old in new List<Collider>(_blockedOutlines.Keys))
            {
                if (!newSet.Contains(old))
                {
                    if (_blockedOutlines[old]) Destroy(_blockedOutlines[old].gameObject);
                    _blockedOutlines.Remove(old);
                }
            }

            foreach (var c in newBlockers)
            {
                if (!_blockedOutlines.ContainsKey(c)) _blockedOutlines[c] = CreateOutlineRenderer(c, Color.red);
            }
        }

        private LineRenderer CreateOutlineRenderer(Collider col, Color color)
        {
            GameObject go = new("BlockerOutline");
            var lr = go.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = lr.endWidth = 0.05f;
            lr.startColor = lr.endColor = color;
            lr.loop = false;
            lr.useWorldSpace = true;
            go.transform.SetParent(col.transform, false);
            if (col is BoxCollider box) DrawOutlineForBoxCollider(box, lr, color, 0.02f);
            return lr;
        }

        private void DrawOutlineForBoxCollider(BoxCollider box, LineRenderer lr, Color color, float yOffset = 0f)
        {
            Vector3 half = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
            Vector3 adjustedCenter = new Vector3(box.center.x, yOffset, box.center.z);
            Vector3[] corners = new Vector3[5];
            corners[0] = box.transform.TransformPoint(adjustedCenter + new Vector3(-half.x, 0f, -half.z));
            corners[1] = box.transform.TransformPoint(adjustedCenter + new Vector3(half.x, 0f, -half.z));
            corners[2] = box.transform.TransformPoint(adjustedCenter + new Vector3(half.x, 0f, half.z));
            corners[3] = box.transform.TransformPoint(adjustedCenter + new Vector3(-half.x, 0f, half.z));
            corners[4] = corners[0];
            lr.positionCount = corners.Length;
            lr.startColor = lr.endColor = color;
            lr.SetPositions(corners);
        }

        public void RepositionBuilding(GameObject building)
        {
            if (_isPlacing) CancelPlacement();

            _ismoving = true;
            _currentBuilding = building;

            StartPlacement(building);
        }

        public void StartPlacement(GameObject prefab)
        {
            if (_isPlacing) CancelPlacement();
            _currentPrefab = prefab;
            _previewInstance = Instantiate(prefab, buildingsParentGameObject.transform);

            if (_ismoving)
            {
                _currentBuilding.SetActive(false); //hide original
            }

            SetPreviewMaterial(_previewInstance);
            _previewRenderers = _previewInstance.GetComponentsInChildren<Renderer>();
            _previewOutline = _previewInstance.AddComponent<LineRenderer>();
            _previewOutline.loop = false;
            _previewOutline.startWidth = _previewOutline.endWidth = 0.05f;
            _previewOutline.material = new Material(Shader.Find("Sprites/Default"));
            _previewOutline.startColor = _previewOutline.endColor = Color.red;
            _previewOutline.useWorldSpace = true;
            _isPlacing = true;

            buildPlacingControls.SetActive(true);
        }

        private void PlaceBuilding(Vector3 position)
        {
            if (_ismoving)
            {
                // move original building to new position
                _currentBuilding.transform.position = position;
                _currentBuilding.transform.rotation = _previewInstance.transform.rotation;
                _currentBuilding.SetActive(true);
            }
            else
            {
                // normal placement (pay resources)
                var buildingData = _currentPrefab.GetComponent<BuildingData>();
                if (!ResourceManager.Instance.HasSufficientResources(buildingData))
                {
                    FloatingTextController.Instance.ShowFloatingText("Not enough resources!", Color.red);
                    return;
                }

                ResourceManager.Instance.DeductResources(buildingData);
                Instantiate(_currentPrefab, position, _previewInstance.transform.rotation, buildingsParentGameObject.transform);
            }

            Destroy(_previewInstance);
            ClearBlockerOutlines();
            _isPlacing = false;
            _ismoving = false;
            buildPlacingControls.SetActive(false);
        }

        private void CancelPlacement()
        {
            ClearBlockerOutlines();
            if (_previewInstance) Destroy(_previewInstance);

            if (_ismoving)
            {
                _currentBuilding.SetActive(true);
            }

            _isPlacing = false;
            _ismoving = false;
            buildPlacingControls.SetActive(false);
        }

        private void RotatePreview()
        {
            if (!_previewInstance) return;
            _previewInstance.transform.Rotate(Vector3.up, 90f * Time.deltaTime);
        }

        private void ClearBlockerOutlines()
        {
            foreach (var kvp in _blockedOutlines)
                if (kvp.Value)
                    Destroy(kvp.Value.gameObject);
            _blockedOutlines.Clear();
        }

        private void SetPreviewMaterial(GameObject obj)
        {
            foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
            {
                var mat = new Material(r.material);
                mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.5f);
                r.material = mat;
            }
        }

        private void SetPreviewColor(Color color)
        {
            foreach (var r in _previewRenderers)
            {
                Color c = r.material.color;
                r.material.color = new Color(color.r, color.g, color.b, c.a);
            }
        }

        private bool IsPointerOverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();


        public void DeleteBuilding(GameObject building)
        {
            if (building == null) return;

            building.TryGetComponent(out BuildingData buildingData);

            if (buildingData != null)
            {
               List<ResourceCost> refunds = ResourceManager.Instance.RefundResourcesPartially(buildingData);
               
               foreach (var refund in refunds)
               {
                   string message = $"Refunded {refund.amount} {refund.resource}";
                   FloatingTextController.Instance.ShowFloatingText(message, Color.green);
               }
            }

            Destroy(building);
        }
    }
}