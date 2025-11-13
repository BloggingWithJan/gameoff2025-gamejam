using System;
using System.Collections.Generic;
using System.Linq;
using GameJam.Core;
using Military;
using Production;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Core
{
    public class SelectionManager : MonoBehaviour
    {
        public List<ISelectable> selectedEntities = new List<ISelectable>();
        public InputActionAsset PlayerActionAsset;
        private InputAction _pointAction;
        private InputAction _clickAction;
        private InputAction _rightClickAction;
        private ISelectable _currentHoveredEntity;

        [SerializeField]
        private SelectionBoxUI selectionBoxUI;

        [SerializeField]
        private Camera mainCamera;

        private Vector2 dragStart;
        private bool isDragging;
        
        private readonly HashSet<string> ignoredTags = new()
        {
            "Building"
        };

        private void OnEnable()
        {
            var map = PlayerActionAsset.FindActionMap("Player");

            _pointAction = map.FindAction("Point");
            _pointAction.Enable();

            _clickAction = map.FindAction("Click");
            _clickAction.Enable();

            _rightClickAction = map.FindAction("RightClick");
            _rightClickAction.Enable();
        }

        private void OnDisable()
        {
            _pointAction.Disable();
            _clickAction.Disable();
            _rightClickAction.Disable();
        }

        void Update()
        {
            HandleBoxSelection();
            if (!isDragging) // Only handle click selection when not dragging
            {
                HandleClickSelection();
                UpdateHoverFeedback();
            }
            HandleRightClick();
        }

        private void HandleRightClick()
        {
            Boolean interaction = false;
            if (_rightClickAction.WasPerformedThisFrame())
            {
                Ray ray = Camera.main.ScreenPointToRay(_pointAction.ReadValue<Vector2>());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    GameObject target = hit.collider.gameObject;

                    if (target.GetComponent<ProductionBuilding>() != null)
                    {
                        interaction = true;
                    }
                    if (target.GetComponent<MilitaryBuilding>() != null)
                    {
                        interaction = true;
                    }
                    if (target.tag == "Enemy")
                    {
                        interaction = true;
                    }

                    // Iterate over a copy of the list to safely remove destroyed items
                    foreach (var entity in selectedEntities.ToList())
                    {
                        // Skip and clean up destroyed objects
                        if (entity == null || (entity as UnityEngine.Object) == null)
                        {
                            selectedEntities.Remove(entity);
                            continue;
                        }

                        IUnitCommandable commandable = (entity as MonoBehaviour)?.GetComponent<IUnitCommandable>();
                        if (commandable != null)
                        {
                            if (interaction)
                                commandable.InteractWith(target);
                            else
                                commandable.MoveTo(hit.point);
                        }
                    }
                }
            }
        }

        private void UpdateHoverFeedback()
        {
            Ray ray = Camera.main.ScreenPointToRay(_pointAction.ReadValue<Vector2>());
            ISelectable newHoveredEntity = null;

            // Find what we're hovering over
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                newHoveredEntity = hit.collider.GetComponent<ISelectable>();
            }

            // If hover target changed
            if (newHoveredEntity != _currentHoveredEntity)
            {
                // Clear previous hover (but only if not selected)
                if (_currentHoveredEntity != null && !_currentHoveredEntity.IsSelected)
                {
                    _currentHoveredEntity.OnUnhover();
                }

                // Apply new hover
                if (newHoveredEntity != null && !newHoveredEntity.IsSelected)
                {
                    newHoveredEntity.OnHover();
                }

                _currentHoveredEntity = newHoveredEntity;
            }
        }

        private void HandleClickSelection()
        {
            if (_clickAction.WasPerformedThisFrame())
            {
                Ray ray = Camera.main.ScreenPointToRay(_pointAction.ReadValue<Vector2>());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    ISelectable selectable = hit.collider.GetComponent<ISelectable>();
                    if (selectable != null)
                    {
                        foreach (var entity in selectedEntities)
                        {
                            entity.OnDeselect();
                        }
                        selectedEntities.Clear();

                        selectable.OnSelect();
                        selectedEntities.Add(selectable);
                    }
                }
            }
        }

        private void HandleBoxSelection()
        {
            Vector2 mousePos = _pointAction.ReadValue<Vector2>();

            // Start dragging
            if (_clickAction.WasPressedThisFrame())
            {
                dragStart = mousePos;
            }

            // During drag
            if (_clickAction.IsPressed())
            {
                if (!isDragging)
                {
                    // Start box selection
                    isDragging = true;
                    selectionBoxUI.StartSelection(dragStart);
                }

                if (isDragging)
                {
                    selectionBoxUI.UpdateSelection(mousePos);
                }
            }

            // End dragging
            if (_clickAction.WasReleasedThisFrame() && isDragging)
            {
                isDragging = false;
                PerformBoxSelection();
                selectionBoxUI.EndSelection();
            }
        }

        private void PerformBoxSelection()
        {
            // Clear previous selection
            foreach (var entity in selectedEntities)
            {
                entity.OnDeselect();
            }
            selectedEntities.Clear();

            // Get screen rect from selection box
            Rect selectionRect = selectionBoxUI.GetScreenRect();

            // Find all selectable objects in the scene
            ISelectable[] allSelectables = FindObjectsByType<MonoBehaviour>(
                    FindObjectsSortMode.None
                )
                .OfType<ISelectable>()
                .ToArray();

            foreach (ISelectable selectable in allSelectables)
            {
                // Get the MonoBehaviour component to access transform
                MonoBehaviour selectableMono = selectable as MonoBehaviour;
                if (selectableMono == null)
                    continue;

                //skip if tag is in ignored list
                if (ignoredTags.Contains(selectableMono.tag))
                    continue;
                
                // Convert world position to screen position
                Vector3 screenPos = mainCamera.WorldToScreenPoint(
                    selectableMono.transform.position
                );

                // Check if within selection box (and in front of camera)
                if (screenPos.z > 0 && selectionRect.Contains(screenPos))
                {
                    selectable.OnSelect();
                    selectedEntities.Add(selectable);
                }
            }
        }
    }
}
