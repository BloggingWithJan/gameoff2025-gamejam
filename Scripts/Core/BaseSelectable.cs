using GameJam.Core;
using UnityEngine;

namespace Core
{
    public class BaseSelectable : MonoBehaviour, ISelectable
    {
        [SerializeField]
        private Color outlineColor = Color.white;

        [SerializeField]
        private float outlineWidth = .1f;

        private Shader outlineShader;

        private bool isSelected = false;
        public bool IsSelected => isSelected; //💩

        private Renderer[] unitRenderers;
        private Material[][] originalMaterials; // Store original materials for each renderer
        private Material[] outlineMaterials;
        private static readonly int OutlineColorProperty = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthProperty = Shader.PropertyToID("_OutlineWidth");

        private void Awake()
        {
            outlineShader = Shader.Find("Custom/SelectableShader");
            unitRenderers = GetComponentsInChildren<Renderer>();

            if (unitRenderers.Length > 0)
            {
                originalMaterials = new Material[unitRenderers.Length][];
                outlineMaterials = new Material[unitRenderers.Length];

                // Store original materials and prepare outline materials
                for (int i = 0; i < unitRenderers.Length; i++)
                {
                    // Save original materials
                    originalMaterials[i] = unitRenderers[i].sharedMaterials;

                    // Create outline material from shader
                    outlineMaterials[i] = new Material(outlineShader);
                    outlineMaterials[i].SetFloat(OutlineWidthProperty, 0f); // Start hidden
                }
            }
        }

        public void OnSelect()
        {
            if (isSelected)
                return;
            isSelected = true;
            Highlight();
        }

        public void OnDeselect()
        {
            if (!isSelected)
                return;
            isSelected = false;
            UnHighlight();
        }

        public void OnHover()
        {
            if (!isSelected)
            {
                ShowHoverOutline();
            }
        }

        public void OnUnhover()
        {
            if (!isSelected)
            {
                DeactivateOutline();
            }
        }

        public void Highlight()
        {
            ActivateOutline();
        }

        public void UnHighlight()
        {
            DeactivateOutline();
        }

        private void ShowHoverOutline()
        {
            ActivateOutline();
        }

        private void ActivateOutline()
        {
            if (outlineMaterials != null && unitRenderers != null)
            {
                for (int i = 0; i < unitRenderers.Length; i++)
                {
                    if (outlineMaterials[i] != null)
                    {
                        // Set hover outline properties
                        outlineMaterials[i].SetColor(OutlineColorProperty, outlineColor);
                        outlineMaterials[i].SetFloat(OutlineWidthProperty, outlineWidth * 0.5f);

                        // Add outline material to existing materials
                        Material[] mats = new Material[originalMaterials[i].Length + 1];
                        originalMaterials[i].CopyTo(mats, 0);
                        mats[mats.Length - 1] = outlineMaterials[i];
                        unitRenderers[i].materials = mats;
                    }
                }
            }
        }

        private void DeactivateOutline()
        {
            if (originalMaterials != null && unitRenderers != null)
            {
                for (int i = 0; i < unitRenderers.Length; i++)
                {
                    if (unitRenderers[i] != null && originalMaterials[i] != null)
                    {
                        unitRenderers[i].materials = originalMaterials[i];
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Clean up outline material instances
            if (outlineMaterials != null)
            {
                foreach (Material mat in outlineMaterials)
                {
                    if (mat != null)
                    {
                        Destroy(mat);
                    }
                }
            }
        }

        public bool IsDead()
        {
            var health = GetComponent<Health>();
            return health != null && health.IsDead();
        }
    }
}
