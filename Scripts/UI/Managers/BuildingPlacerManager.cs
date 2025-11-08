using System.Collections.Generic;
using Resource;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI.Managers
{
    public class BuildingPlacerManager : MonoBehaviour
    {
        public LayerMask groundMask;

        private GameObject currentPrefab;
        private GameObject previewInstance;
        private bool placing;
        private Renderer[] previewRenderers;
        private LineRenderer previewOutline;

        private readonly Dictionary<Collider, LineRenderer> blockedOutlines = new();
        private Vector2? lastMousePos;
        private float rotationSpeed = 0.3f;

        void Update()
        {
            if (!placing || Mouse.current == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask)) return;

            previewInstance.transform.position = hit.point;

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelPlacement();
                return;
            }

            bool overUI = IsPointerOverUI();
            var (valid, blockers) = IsValidPlacement(hit.point);

            Color previewColor = (overUI || !valid) ? Color.red : Color.green;
            SetPreviewColor(previewColor);

            DrawOutlineForBoxCollider(previewInstance.GetComponent<BoxCollider>(), previewOutline, previewColor, 0.02f);

            UpdateBlockingOutlines(blockers);

            if (Keyboard.current.rKey.isPressed)
            {
                RotatePreview();
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (overUI)
                {
                    FloatingTextManager.Instance.ShowFloatingText("Cannot place over UI!");
                    return;
                }

                if (!valid)
                {
                    FloatingTextManager.Instance.ShowFloatingText("Placement blocked!");
                    return;
                }

                PlaceBuilding(hit.point);
            }
        }

        private (bool valid, List<Collider> blockers) IsValidPlacement(Vector3 position)
        {
            List<Collider> blocking = new();
            if (!previewInstance) return (true, blocking);

            BoxCollider box = previewInstance.GetComponent<BoxCollider>();
            if (!box) return (true, blocking);

            box.enabled = false;

            Vector3 worldCenter = position + previewInstance.transform.rotation * box.center;
            Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, previewInstance.transform.lossyScale);
            Collider[] overlaps = Physics.OverlapBox(worldCenter, halfExtents, previewInstance.transform.rotation);

            box.enabled = true;

            foreach (Collider c in overlaps)
            {
                if (c.isTrigger || c.CompareTag("Ground") || c.gameObject == previewInstance) continue;
                blocking.Add(c);
            }

            return (blocking.Count == 0, blocking);
        }

        private void UpdateBlockingOutlines(List<Collider> newBlockers)
        {
            HashSet<Collider> newSet = new(newBlockers);

            foreach (var old in new List<Collider>(blockedOutlines.Keys))
            {
                if (!newSet.Contains(old))
                {
                    if (blockedOutlines[old]) Destroy(blockedOutlines[old].gameObject);
                    blockedOutlines.Remove(old);
                }
            }

            foreach (var c in newBlockers)
            {
                if (!blockedOutlines.ContainsKey(c))
                    blockedOutlines[c] = CreateOutlineRendererFor(c, Color.red);
            }
        }

        private LineRenderer CreateOutlineRendererFor(Collider col, Color color)
        {
            GameObject go = new("BlockerOutline");
            var lr = go.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = lr.endWidth = 0.05f;
            lr.startColor = lr.endColor = color;
            lr.loop = false;
            lr.useWorldSpace = true;
            go.transform.SetParent(col.transform, false);

            if (col is BoxCollider box)
                DrawOutlineForBoxCollider(box, lr, color, 0.02f);

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

        public void StartPlacement(GameObject prefab)
        {
            if (placing) CancelPlacement();

            currentPrefab = prefab;
            previewInstance = Instantiate(prefab);
            SetPreviewMaterial(previewInstance);
            previewRenderers = previewInstance.GetComponentsInChildren<Renderer>();

            previewOutline = previewInstance.AddComponent<LineRenderer>();
            previewOutline.loop = false;
            previewOutline.startWidth = previewOutline.endWidth = 0.05f;
            previewOutline.material = new Material(Shader.Find("Sprites/Default"));
            previewOutline.startColor = previewOutline.endColor = Color.red;
            previewOutline.useWorldSpace = true;

            placing = true;
        }

        private void PlaceBuilding(Vector3 position)
        {
            // Check if player has enough resources first
            var buildingData = currentPrefab.GetComponent<BuildingData>();
            if (buildingData == null)
            {
                Debug.LogError($"Can't find BuildingData component on GameObject {gameObject.name}");
            }

            bool hasSufficientResources = ResourceManager.Instance.HasSufficientResources(buildingData);
            if (!hasSufficientResources)
            {
                FloatingTextManager.Instance.ShowFloatingText("Not enough resources!");
                return;
            }

            ResourceManager.Instance.DeductResources(buildingData);

            // Instantiate building
            GameObject instantiatedObject = Instantiate(currentPrefab, position, Quaternion.identity);
            instantiatedObject.transform.rotation = previewInstance.transform.rotation;
            ClearBlockerOutlines();
            Destroy(previewInstance);
            placing = false;
        }

        private void CancelPlacement()
        {
            ClearBlockerOutlines();
            if (previewInstance) Destroy(previewInstance);
            placing = false;
        }

        private void RotatePreview()
        {
            if (!previewInstance) return;

            previewInstance.transform.Rotate(Vector3.up, Space.Self);
        }

        private void ClearBlockerOutlines()
        {
            foreach (var kvp in blockedOutlines)
                if (kvp.Value)
                    Destroy(kvp.Value.gameObject);
            blockedOutlines.Clear();
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
            foreach (var r in previewRenderers)
            {
                Color c = r.material.color;
                r.material.color = new Color(color.r, color.g, color.b, c.a);
            }
        }

        private bool IsPointerOverUI() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}