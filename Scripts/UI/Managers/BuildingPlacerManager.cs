using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI.Managers
{
    public class BuildingPlacerManager : MonoBehaviour
    {
        public GameObject basePrefab;
        public GameObject quarryPrefab;
        public GameObject rockPrefab;
        public GameObject treePrefab;
        public LayerMask groundMask;

        private GameObject currentPrefab;
        private GameObject previewInstance;
        private bool placing;
        private Renderer[] previewRenderers;
        private LineRenderer outlineRenderer;

        private readonly Dictionary<Collider, LineRenderer> blockedOutlines = new();

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

            if (IsPointerOverUI())
            {
                SetPreviewColor(Color.red);
                DrawColliderOutline(Color.red);
                ClearBlockerOutlines();
                return;
            }

            var (valid, blockers) = IsValidPlacement(hit.point);
            Color previewColor = valid ? Color.green : Color.red;
            SetPreviewColor(previewColor);
            DrawColliderOutline(previewColor);

            UpdateBlockingOutlines(blockers);

            if (valid && Mouse.current.leftButton.wasPressedThisFrame)
            {
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

            // Remove outlines from old blockers that are no longer colliding
            foreach (var old in new List<Collider>(blockedOutlines.Keys))
            {
                if (!newSet.Contains(old))
                {
                    if (blockedOutlines[old])
                        Destroy(blockedOutlines[old].gameObject);
                    blockedOutlines.Remove(old);
                }
            }

            // Add outlines to new blockers
            foreach (var c in newBlockers)
            {
                if (!blockedOutlines.ContainsKey(c))
                {
                    var lr = CreateOutlineRendererFor(c);
                    blockedOutlines[c] = lr;
                }
            }
        }

        private LineRenderer CreateOutlineRendererFor(Collider col)
        {
            GameObject go = new("BlockerOutline");
            var lr = go.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = lr.endWidth = 0.05f;
            lr.startColor = lr.endColor = Color.red;
            lr.loop = false;
            lr.useWorldSpace = true;
            go.transform.SetParent(col.transform, false);

            if (col is BoxCollider box)
                DrawOutlineForCollider(box, lr, Color.red);

            return lr;
        }

        private void DrawOutlineForCollider(BoxCollider box, LineRenderer lr, Color color)
        {
            Vector3 center = box.center;
            Vector3 size = box.size;
            Vector3 half = Vector3.Scale(size * 0.5f, box.transform.lossyScale);

            Vector3[] corners = new Vector3[5];
            corners[0] = box.transform.TransformPoint(center + new Vector3(-half.x, -half.y, -half.z));
            corners[1] = box.transform.TransformPoint(center + new Vector3(half.x, -half.y, -half.z));
            corners[2] = box.transform.TransformPoint(center + new Vector3(half.x, -half.y, half.z));
            corners[3] = box.transform.TransformPoint(center + new Vector3(-half.x, -half.y, half.z));
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

            outlineRenderer = previewInstance.AddComponent<LineRenderer>();
            outlineRenderer.loop = false;
            outlineRenderer.startWidth = outlineRenderer.endWidth = 0.05f;
            outlineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            outlineRenderer.startColor = outlineRenderer.endColor = Color.red;
            outlineRenderer.useWorldSpace = true;

            placing = true;
        }

        private void PlaceBuilding(Vector3 position)
        {
            Instantiate(currentPrefab, position, Quaternion.identity);
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

        private void DrawColliderOutline(Color color)
        {
            if (!previewInstance) return;
            BoxCollider box = previewInstance.GetComponent<BoxCollider>();
            if (!box) return;

            Vector3 center = box.center;
            Vector3 size = box.size;
            Vector3 lossyScale = previewInstance.transform.lossyScale;
            Vector3 half = Vector3.Scale(size * 0.5f, lossyScale);

            Vector3[] localCorners = new Vector3[]
            {
                new Vector3(-half.x, -half.y, -half.z),
                new Vector3(half.x, -half.y, -half.z),
                new Vector3(half.x, -half.y, half.z),
                new Vector3(-half.x, -half.y, half.z)
            };

            Vector3[] worldCorners = new Vector3[localCorners.Length + 1];

            for (int i = 0; i < localCorners.Length; i++)
            {
                Vector3 corner = previewInstance.transform.TransformPoint(center + localCorners[i]);
                if (Physics.Raycast(corner + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f, groundMask))
                    worldCorners[i] = hit.point + Vector3.up * 0.02f;
                else
                    worldCorners[i] = corner;
            }

            worldCorners[worldCorners.Length - 1] = worldCorners[0];
            outlineRenderer.positionCount = worldCorners.Length;
            outlineRenderer.startColor = outlineRenderer.endColor = color;
            outlineRenderer.SetPositions(worldCorners);
        }

        public void OnClickQuarry() => StartPlacement(quarryPrefab);
        public void OnClickBase() => StartPlacement(basePrefab);
        public void OnClickRock() => StartPlacement(rockPrefab);
        public void OnClickTree() => StartPlacement(treePrefab);
    }
}
