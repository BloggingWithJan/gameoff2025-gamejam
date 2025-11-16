using System.Collections.Generic;
using Controller.UI;
using Data;
using Resource;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace UI.Managers
{
    public class BuildingPlacementController : MonoBehaviour
    {
        public InputActionAsset InputActionAsset;
        public LayerMask groundMask;
        public GameObject buildPlacingControls;
        public GameObject buildingsParentGameObject;

        private GameObject _currentPrefab;
        private GameObject _previewInstance;
        private bool _isPlacing;

        //if a building gets moved
        private GameObject _currentBuilding;
        private bool _isBeingRelocated;

        private Renderer[] _previewRenderers;
        private LineRenderer _previewOutline;
        private readonly Dictionary<Collider, LineRenderer> _blockedOutlines = new();
        private Vector2? _lastMousePos;
        private float _rotationSpeed = 90f;
        private float _maxRaycastDistance = 500f;

        // distance used when sampling the NavMesh to determine if placement point is on a navmesh
        [SerializeField] private float navMeshSampleDistance = 0.5f;

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
            if (!_isPlacing)
                return;

            if (Mouse.current == null || !TryGetGroundHit(out RaycastHit hit))
            {
                Debug.LogError("No ground hit");
                return;
            }

            _previewInstance.transform.position = hit.point;

            bool isOverUI = IsPointerOverUI();
            var (noBlockers, onNavMesh, blockers) = IsValidPlacement(hit.point);

            UpdateVisuals(noBlockers && !isOverUI && onNavMesh, blockers);

            HandleInput(hit, isOverUI, noBlockers, onNavMesh);
        }

        private void UpdateVisuals(bool canPlace, List<Collider> blockers)
        {
            Color previewColor = canPlace ? Color.green : Color.red;
            SetPreviewColor(previewColor);

            if (_previewInstance.TryGetComponent<BoxCollider>(out var boxCollider))
            {
                DrawOutlineForBoxCollider(boxCollider, _previewOutline, previewColor, 0.02f);
            }

            UpdateBlockingOutlines(blockers);
        }

        private void HandleInput(RaycastHit hit, bool overUI, bool noBlockers, bool onNavMesh)
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

                if (!noBlockers)
                {
                    FloatingTextController.Instance.ShowFloatingText("Placement blocked!", Color.red);
                    return;
                }
                
                if (!onNavMesh)
                {
                    FloatingTextController.Instance.ShowFloatingText("This location is not buildable.",
                        Color.red);
                    return;
                }

                PlaceBuilding(hit.point);
            }
        }

        private (bool noBlockers, bool onNavMesh, List<Collider> blockers) IsValidPlacement(Vector3 position)
        {
            var blockers = new List<Collider>();

            if (!_previewInstance.TryGetComponent<BoxCollider>(out var box))
                return (false, false, blockers);

            // Disable collider temporarily
            box.enabled = false;

            Vector3 worldCenter = position + _previewInstance.transform.rotation * box.center;
            Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, _previewInstance.transform.lossyScale);

            Collider[] overlaps = Physics.OverlapBox(worldCenter, halfExtents, _previewInstance.transform.rotation);

            box.enabled = true;

            foreach (var c in overlaps)
            {
                if (c.isTrigger || c.CompareTag("Ground") || c.gameObject == _previewInstance)
                    continue;

                blockers.Add(c);
            }

            bool noBlockers = blockers.Count == 0;

            bool fullyOnNavMesh = IsFullyOnNavMesh(box, 1f);

            return (noBlockers, fullyOnNavMesh, blockers);
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

            _isBeingRelocated = true;
            _currentBuilding = building;

            StartPlacement(building);
        }

        public void StartPlacement(GameObject prefab)
        {
            if (_isPlacing)
                CancelPlacement();

            _currentPrefab = prefab;
            _previewInstance = Instantiate(prefab, buildingsParentGameObject.transform);

            //disable obstacle so that units dont get pushed around
            if (_previewInstance.TryGetComponent<NavMeshObstacle>(out NavMeshObstacle obstacle))
            {
                obstacle.enabled = false;
            }

            if (_isBeingRelocated)
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

            DisableActionAssets();
        }

        private void DisableActionAssets()
        {
            var map = InputActionAsset.FindActionMap("Player");
            map.FindAction("Click").Disable();
        }

        private void ReenableActionAssets()
        {
            var map = InputActionAsset.FindActionMap("Player");
            map.FindAction("Click").Enable();
        }

        private void PlaceBuilding(Vector3 position)
        {
            if (_isBeingRelocated)
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

                foreach (var cost in buildingData.costs)
                {
                    string message = $"- {cost.amount} {cost.resource}";
                    FloatingTextController.Instance.ShowFloatingText(message, Color.white);
                }

                Instantiate(_currentPrefab, position, _previewInstance.transform.rotation,
                    buildingsParentGameObject.transform);
            }

            Destroy(_previewInstance);
            ClearBlockerOutlines();
            ReenableActionAssets();
            _isPlacing = false;
            _isBeingRelocated = false;
            buildPlacingControls.SetActive(false);
        }

        private void CancelPlacement()
        {
            ClearBlockerOutlines();
            if (_previewInstance) Destroy(_previewInstance);

            if (_isBeingRelocated)
            {
                _currentBuilding.SetActive(true);
            }

            ReenableActionAssets();
            _isPlacing = false;
            _isBeingRelocated = false;
            buildPlacingControls.SetActive(false);
        }

        private void RotatePreview()
        {
            if (!_previewInstance) return;
            _previewInstance.transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
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

        private bool IsFullyOnNavMesh(BoxCollider box, float maxDistance)
        {
            // Get all 4 bottom corners in world space
            Vector3 half = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);

            Vector3[] localOffsets =
            {
                new Vector3(-half.x, 0, -half.z),
                new Vector3(half.x, 0, -half.z),
                new Vector3(half.x, 0, half.z),
                new Vector3(-half.x, 0, half.z)
            };

            foreach (var offset in localOffsets)
            {
                Vector3 worldPoint = box.transform.TransformPoint(box.center + offset);

                // Every corner must be on NavMesh
                if (!NavMesh.SamplePosition(worldPoint, out _, maxDistance, NavMesh.AllAreas))
                    return false;
            }

            return true;
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
                    string message = $"+ {refund.amount} {refund.resource}";
                    FloatingTextController.Instance.ShowFloatingText(message, Color.green);
                }
            }

            Destroy(building);
        }
    }
}