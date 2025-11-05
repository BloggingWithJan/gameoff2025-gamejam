using System;
using System.Collections.Generic;
using GameJam.Worker;
using UnityEngine;

namespace GameJam.Resource
{
    public class ResourceNode : MonoBehaviour
    {
        private class GatherSlot
        {
            public Vector3 offset;
            public Gatherer assignedGatherer;

            public bool IsOccupied() => assignedGatherer != null;
        }

        public event Action OnResourceDepleted;

        public int maxResourceAmount = 100;
        public int currentResourceAmount;

        public int numberOfGatherSlots = 4;

        public float gatherRadius = 2f;

        private List<GatherSlot> gatherSlots = new List<GatherSlot>();

        private void Awake()
        {
            if (gatherSlots.Count == 0)
            {
                GenerateGatherOffsets();
            }
            currentResourceAmount = maxResourceAmount;
        }

        private void GenerateGatherOffsets()
        {
            gatherSlots.Clear();

            for (int i = 0; i < numberOfGatherSlots; i++)
            {
                float angle = (360f / numberOfGatherSlots) * i;
                float radians = angle * Mathf.Deg2Rad;

                Vector3 offset = new Vector3(
                    Mathf.Cos(radians) * gatherRadius,
                    0f,
                    Mathf.Sin(radians) * gatherRadius
                );

                gatherSlots.Add(new GatherSlot { offset = offset });
            }
        }

        // public bool TryReserveSlot(WorkerUnit worker)
        // {
        //     for (int i = 0; i < gatherSlots.Count; i++)
        //     {
        //         if (!gatherSlots[i].IsOccupied())
        //         {
        //             gatherSlots[i].assignedWorker = worker;
        //             return true;
        //         }
        //     }

        //     return false;
        // }
        public bool TryReserveSlot(Gatherer gatherer)
        {
            int nearestSlotIndex = -1;
            float nearestDistance = float.MaxValue;

            // Find the nearest available slot
            for (int i = 0; i < gatherSlots.Count; i++)
            {
                if (!gatherSlots[i].IsOccupied())
                {
                    Vector3 slotWorldPos = transform.position + gatherSlots[i].offset;
                    float distance = Vector3.Distance(gatherer.transform.position, slotWorldPos);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestSlotIndex = i;
                    }
                }
            }

            // Assign the nearest slot if found
            if (nearestSlotIndex >= 0)
            {
                gatherSlots[nearestSlotIndex].assignedGatherer = gatherer;
                return true;
            }

            return false;
        }

        public Vector3 GetSlotPosition(Gatherer gatherer)
        {
            int slotIndex = gatherSlots.FindIndex(slot => slot.assignedGatherer == gatherer);
            if (slotIndex >= 0 && slotIndex < gatherSlots.Count)
            {
                return transform.position + gatherSlots[slotIndex].offset;
            }
            return transform.position;
        }

        public void ReleaseSlot(Gatherer gatherer)
        {
            int slotIndex = gatherSlots.FindIndex(slot => slot.assignedGatherer == gatherer);
            if (slotIndex >= 0 && slotIndex < gatherSlots.Count)
            {
                gatherSlots[slotIndex].assignedGatherer = null;
            }
        }

        /// Get direction from the gatherer's slot to the resource center so the gatherer can face the resource while gathering
        public Vector3 GetFacingDirection(Gatherer gatherer)
        {
            int slotIndex = gatherSlots.FindIndex(slot => slot.assignedGatherer == gatherer);
            if (slotIndex >= 0 && slotIndex < gatherSlots.Count)
            {
                Vector3 slotPos = transform.position + gatherSlots[slotIndex].offset;
                return (transform.position - slotPos).normalized;
            }
            return Vector3.forward;
        }

        public bool CanGather(int amount) => currentResourceAmount >= amount;

        public int Gather(int amount)
        {
            int gathered = Mathf.Min(amount, currentResourceAmount);
            currentResourceAmount -= gathered;
            if (currentResourceAmount <= 0)
            {
                ResourceDepleted();
            }
            return gathered;
        }

        private void ResourceDepleted()
        {
            OnResourceDepleted?.Invoke();
            Destroy(gameObject);
        }

        ///////////////////////////////////////////
        /// Gizmos Drawing - Fully AI generated ///
        ///////////////////////////////////////////
        private void OnDrawGizmos()
        {
            // Draw the gather radius circle
            Gizmos.color = Color.yellow;
            DrawCircle(transform.position, gatherRadius, 32);

            // Draw gather slots
            if (Application.isPlaying && gatherSlots != null && gatherSlots.Count > 0)
            {
                // Draw slots during play mode with occupation status
                for (int i = 0; i < gatherSlots.Count; i++)
                {
                    Vector3 worldPos = transform.position + gatherSlots[i].offset;

                    // Color based on occupied state
                    Gizmos.color = gatherSlots[i].IsOccupied() ? Color.red : Color.green;
                    Gizmos.DrawSphere(worldPos, 0.3f);

                    // Draw line from center to slot
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(transform.position, worldPos);

                    // Draw slot number
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(
                        worldPos + Vector3.up * 0.5f,
                        gatherSlots[i].IsOccupied() ? $"{i} (Occupied)" : $"{i} (Free)"
                    );
#endif
                }
            }
            else
            {
                // Preview positions in edit mode
                for (int i = 0; i < numberOfGatherSlots; i++)
                {
                    float angle = (360f / numberOfGatherSlots) * i;
                    float radians = angle * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(
                        Mathf.Cos(radians) * gatherRadius,
                        0f,
                        Mathf.Sin(radians) * gatherRadius
                    );
                    Vector3 worldPos = transform.position + offset;

                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(worldPos, 0.3f);

                    // Draw line from center to slot
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(transform.position, worldPos);

                    // Draw slot number
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(worldPos + Vector3.up * 0.5f, i.ToString());
#endif
                }
            }
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 newPoint =
                    center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}
