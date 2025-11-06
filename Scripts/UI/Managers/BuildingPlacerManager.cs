using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI.Managers
{
    public class BuildingPlacerManager : MonoBehaviour
    {
        public GameObject basePrefab;
        public GameObject quarryPrefab;
        public LayerMask groundMask;

        private GameObject currentPrefab;
        private GameObject previewInstance;
        private bool placing;
        private Renderer[] previewRenderers;

        void Update()
        {
            if (!placing || Mouse.current == null) return;

            //Ray from camera to mouse  
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask))
            {
                //Move preview **always**  
                previewInstance.transform.position = hit.point;

                //Visual feedback if over UI  
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    SetPreviewColor(Color.red); //blocked  
                else
                    SetPreviewColor(Color.green); //allowed  

                //Place building (only if not over UI)  
                if (Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUI())
                    PlaceBuilding(hit.point);

                //Cancel placement  
                if (Mouse.current.rightButton.wasPressedThisFrame)
                    CancelPlacement();
            }
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
    }
}