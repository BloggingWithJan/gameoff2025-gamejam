using System;
using System.Collections.Generic;
using System.Linq;
using Controller.UI;
using Data;
using GameJam.Core;
using GameJam.Military;
using GameJam.Movement;
using Production;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private float clickPressTime;

        private const float MIN_HOLD_FOR_DRAG = 0.15f;
        private const float MIN_DRAG_DISTANCE = 10f;

        private readonly HashSet<string> ignoredTags = new() { "Building" };

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
            //set to false because otherwise the selection box gets shown after clickAction gets enabled again
            if (!_clickAction.enabled)
            {
                isDragging = false;
            }

            HandleBoxSelection();

            if (!isDragging) // Only handle click selection when not dragging
            {
                HandleClickSelection();
                UpdateHoverFeedback();
            }

            TidyUpSelectedEntities();
            UpdateSelectionUI();
            SetCursorType();
            HandleRightClick();
        }

        private void HandleRightClick()
        {
            Boolean interaction = false;
            if (_rightClickAction.WasPressedThisFrame())
            {
                Debug.Log("Right click detected in SelectionManager");
                Ray ray = Camera.main.ScreenPointToRay(_pointAction.ReadValue<Vector2>());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    GameObject target = hit.collider.gameObject;

                    var prod = target.GetComponent<ProductionBuilding>();
                    if (prod != null)
                    {
                        interaction = true;
                        if (!prod.HasAvailableUnitSlots())
                        {
                            FloatingTextController.Instance.ShowFloatingText(
                                "Unit limit reached. Build another structure.",
                                Color.red
                            );
                            return; // stop the right-click action here
                        }
                    }

                    var mil = target.GetComponent<MilitaryBuilding>();
                    if (mil != null)
                    {
                        interaction = true;
                        if (!mil.HasAvailableUnitSlots())
                        {
                            FloatingTextController.Instance.ShowFloatingText(
                                "Unit limit reached. Build another structure.",
                                Color.red
                            );
                            return; // stop the right-click action here
                        }
                    }

                    if (target.tag == "Enemy")
                        interaction = true;

                    CleanUpDestroyedSelections();
                    if (interaction)
                    {
                        IssueInteractionCommand(hit.point, target);
                    }
                    else
                    {
                        IssueMovementCommand(hit.point);
                    }
                }
            }
        }

        private void CleanUpDestroyedSelections()
        {
            // Iterate over a copy of the list to safely remove destroyed items
            foreach (var entity in selectedEntities.ToList())
            {
                // Skip and clean up destroyed objects
                if (entity == null || (entity as UnityEngine.Object) == null)
                {
                    selectedEntities.Remove(entity);
                }
            }
        }

        private void UpdateHoverFeedback()
        {
            Ray ray = Camera.main.ScreenPointToRay(_pointAction.ReadValue<Vector2>());
            ISelectable newHoveredEntity = null;

            // Find what we're hovering over
            if (Physics.Raycast(ray, out RaycastHit hit))
                newHoveredEntity = hit.collider.GetComponent<ISelectable>();

            // If hover target changed
            if (newHoveredEntity != _currentHoveredEntity)
            {
                // Clear previous hover (but only if not selected)
                if (_currentHoveredEntity != null && !_currentHoveredEntity.IsSelected)
                    _currentHoveredEntity.OnUnhover();

                // Apply new hover
                if (
                    newHoveredEntity != null
                    && !newHoveredEntity.IsSelected
                    && !newHoveredEntity.IsDead()
                )
                    newHoveredEntity.OnHover();

                _currentHoveredEntity = newHoveredEntity;
            }
        }

        private void HandleClickSelection()
        {
            if (_clickAction.WasPressedThisFrame())
            {
                // If pointer is over UI, don't clear selection
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                Ray ray = Camera.main.ScreenPointToRay(_pointAction.ReadValue<Vector2>());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    ISelectable selectable = hit.collider.GetComponent<ISelectable>();
                    if (selectable != null && !selectable.IsDead())
                    {
                        foreach (var entity in selectedEntities)
                            entity.OnDeselect();

                        selectedEntities.Clear();

                        selectable.OnSelect();
                        selectedEntities.Add(selectable);
                    }
                    else
                    {
                        // Clicked on empty space - clear selection
                        foreach (var entity in selectedEntities)
                            entity.OnDeselect();

                        selectedEntities.Clear();
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
                clickPressTime = Time.time; // Store when click started
            }

            // During drag
            if (_clickAction.IsPressed())
            {
                // Calculate how long the button has been held
                float holdDuration = Time.time - clickPressTime;

                // Calculate drag distance
                float dragDistance = Vector2.Distance(dragStart, mousePos);

                if (!isDragging)
                {
                    // Only start box selection if:
                    // 1. Held for minimum time (e.g., 0.15 seconds) OR
                    // 2. Dragged minimum distance (e.g., 10 pixels)
                    if (holdDuration > MIN_HOLD_FOR_DRAG || dragDistance > MIN_DRAG_DISTANCE)
                    {
                        isDragging = true;
                        selectionBoxUI.StartSelection(dragStart);
                    }
                }

                if (isDragging)
                    selectionBoxUI.UpdateSelection(mousePos);
            }

            // End dragging
            if (_clickAction.WasReleasedThisFrame() && isDragging)
            {
                isDragging = false;
                PerformBoxSelection();
                selectionBoxUI.EndSelection();
            }
        }

        private void UpdateSelectionUI()
        {
            BuildingInfoPanel.Instance.HidePanel();
            UnitInfoPanel.Instance.HidePanel();
            ArmyInfoPanel.Instance.HidePanel();

            if (selectedEntities.Count == 0)
                return;

            if (selectedEntities.Count > 1)
            {
                var units = selectedEntities
                    .Select(e => (e as MonoBehaviour)?.GetComponent<Health>())
                    .Where(h => h != null)
                    .ToList();

                if (units.Count > 0)
                {
                    ArmyInfoPanel.Instance.ShowPanel(selectedEntities.Count);
                }

                return;
            }

            MonoBehaviour mb = selectedEntities[0] as MonoBehaviour;

            if (mb.TryGetComponent(out BaseBuilding buildingData))
            {
                BuildingInfoPanel.Instance.ShowPanel(buildingData);
                return;
            }

            if (mb.TryGetComponent(out Health unitData))
            {
                UnitInfoPanel.Instance.ShowPanel(unitData);
                return;
            }
        }

        private void PerformBoxSelection()
        {
            // Clear previous selection
            foreach (var entity in selectedEntities)
                entity.OnDeselect();

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

                if (selectable.IsDead())
                    continue;

                //box selection only for units
                if (selectableMono.GetComponent<Unit>() == null)
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

        public void IssueMovementCommand(Vector3 clickedPosition)
        {
            if (selectedEntities.Count == 1)
            {
                // Single unit - move to clicked position with small offset
                Vector3 offset = UnityEngine.Random.insideUnitSphere * 0.5f;
                offset.y = 0;
                IUnitCommandable commandable = (
                    selectedEntities[0] as MonoBehaviour
                )?.GetComponent<IUnitCommandable>();
                commandable?.MoveTo(clickedPosition, clickedPosition + offset);
            }
            else
            {
                // Multiple units - use formation
                Vector3[] slots = FormationHelper.GetFormationPositions(
                    clickedPosition,
                    selectedEntities.Count
                );

                for (int i = 0; i < selectedEntities.Count; i++)
                {
                    IUnitCommandable commandable = (
                        selectedEntities[i] as MonoBehaviour
                    )?.GetComponent<IUnitCommandable>();
                    commandable?.MoveTo(clickedPosition, slots[i]);
                }
            }
        }

        public void IssueInteractionCommand(Vector3 clickedPosition, GameObject target)
        {
            foreach (var entity in selectedEntities)
            {
                IUnitCommandable commandable = (
                    entity as MonoBehaviour
                )?.GetComponent<IUnitCommandable>();
                commandable?.InteractWith(target);
            }
        }

        private void SetCursorType()
        {
            if (selectedEntities.Count > 0)
            {
                MonoBehaviour mbSelected = selectedEntities[0] as MonoBehaviour;
                Unit mbUnit = mbSelected.GetComponent<Unit>();

                if (_currentHoveredEntity != null)
                {
                    MonoBehaviour mb = _currentHoveredEntity as MonoBehaviour;
                    MilitaryBuilding militaryBuilding = mb.GetComponent<MilitaryBuilding>();
                    ProductionBuilding productionBuilding = mb.GetComponent<ProductionBuilding>();
                    SimpleBuilding simpleBuilding = mb.GetComponent<SimpleBuilding>();

                    if (militaryBuilding != null && mbUnit != null)
                    {
                        if (militaryBuilding.HasAvailableUnitSlots())
                        {
                            CursorHelper.Instance.SetCursor(
                                CursorHelper.CursorType.InteractWithBuilding
                            );
                            return;
                        }
                        else
                        {
                            CursorHelper.Instance.SetCursor(
                                CursorHelper.CursorType.NoInteractionPossible
                            );
                            return;
                        }
                    }

                    if (productionBuilding != null && mbUnit != null)
                    {
                        if (productionBuilding.HasAvailableUnitSlots())
                        {
                            CursorHelper.Instance.SetCursor(
                                CursorHelper.CursorType.InteractWithBuilding
                            );
                            return;
                        }
                        else
                        {
                            CursorHelper.Instance.SetCursor(
                                CursorHelper.CursorType.NoInteractionPossible
                            );
                            return;
                        }
                    }

                    if (simpleBuilding != null && mbUnit != null)
                    {
                        CursorHelper.Instance.SetCursor(
                            CursorHelper.CursorType.NoInteractionPossible
                        );
                        return;
                    }

                    if (mb.tag == "Enemy" && mbUnit != null)
                    {
                        CursorHelper.Instance.SetCursor(CursorHelper.CursorType.Attack);
                        return;
                    }

                    CursorHelper.Instance.SetCursor(CursorHelper.CursorType.Pointer);
                    return;
                }

                if (_currentHoveredEntity == null)
                {
                    if (mbUnit != null)
                    {
                        CursorHelper.Instance.SetCursor(CursorHelper.CursorType.Move);
                        return;
                    }
                    else
                    {
                        CursorHelper.Instance.SetCursor(CursorHelper.CursorType.Pointer);
                        return;
                    }
                }
            }

            if (_currentHoveredEntity != null)
            {
                CursorHelper.Instance.SetCursor(CursorHelper.CursorType.Pointer);
                return;
            }

            CursorHelper.Instance.SetCursor(CursorHelper.CursorType.Default);
        }

        private void TidyUpSelectedEntities()
        {
            // Iterate over a copy of the list to safely remove destroyed items
            foreach (var entity in selectedEntities.ToList())
            {
                // Skip and clean up destroyed objects
                if (entity == null || (entity as UnityEngine.Object) == null || entity.IsDead())
                {
                    selectedEntities.Remove(entity);
                }
            }
        }
    }
}
