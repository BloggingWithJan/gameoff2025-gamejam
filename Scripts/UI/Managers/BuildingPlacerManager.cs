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

        void Update()
        {
            if (!placing || Mouse.current == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask)) return;

            // Move preview to mouse position
            previewInstance.transform.position = hit.point;

            // Cancel placement with right click
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelPlacement();
                return;
            }

            // Skip if pointer is over UI
            if (IsPointerOverUI())
            {
                SetPreviewColor(Color.red);
                return;
            }

            // Validate placement
            bool valid = IsValidPlacement(hit.point);
            SetPreviewColor(valid ? Color.green : Color.red);

            // Place building only if valid
            if (valid && Mouse.current.leftButton.wasPressedThisFrame)
            {
                PlaceBuilding(hit.point);
            }
        }

        private bool IsValidPlacement(Vector3 position)
        {
            if (!previewInstance) return true;

            BoxCollider box = previewInstance.GetComponent<BoxCollider>();
            if (!box) return true;

            box.enabled = false;

            Vector3 worldCenter = position + previewInstance.transform.rotation * box.center;
            Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, previewInstance.transform.lossyScale);

            Collider[] overlaps = Physics.OverlapBox(
                worldCenter,
                halfExtents,
                previewInstance.transform.rotation
            );

            box.enabled = true;

            foreach (Collider c in overlaps)
            {
                if (c.isTrigger || c.CompareTag("Ground"))
                    continue;

                return false; // Blocked by something solid
            }

            return true;
        }

        public void StartPlacement(GameObject prefab)
        {
            if (placing) CancelPlacement();

            currentPrefab = prefab;
            previewInstance = Instantiate(prefab);
            SetPreviewMaterial(previewInstance);
            previewRenderers = previewInstance.GetComponentsInChildren<Renderer>();
            placing = true;
        }

        private void PlaceBuilding(Vector3 position)
        {
            Instantiate(currentPrefab, position, Quaternion.identity);
            Destroy(previewInstance);
            placing = false;
        }

        private void CancelPlacement()
        {
            if (previewInstance) Destroy(previewInstance);
            placing = false;
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

        private bool IsPointerOverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        public void OnClickQuarry() => StartPlacement(quarryPrefab);
        public void OnClickBase() => StartPlacement(basePrefab);
        public void OnClickRock() => StartPlacement(rockPrefab);
        public void OnClickTree() => StartPlacement(treePrefab);
    }
}