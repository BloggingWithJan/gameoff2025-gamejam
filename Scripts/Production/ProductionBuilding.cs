using System.Collections.Generic;
using GameJam.Production;
using Resource;
using UnityEngine;

namespace GameJam.Production
{
    public class ProductionBuilding : MonoBehaviour
    {
        [SerializeField]
        private int maxGathererSlots = 3;

        [SerializeField]
        private GathererType gathererType;

        private List<Gatherer> assignedGatherers = new List<Gatherer>();

        public void DepositResources(int amount)
        {
            if (gathererType.GetResourceTag() == "Stone")
                ResourceManager.Instance.AddResource(ResourceType.Stone, amount);
            else if (gathererType.GetResourceTag() == "Wood")
                ResourceManager.Instance.AddResource(ResourceType.Wood, amount);
        }

        public bool RequestGathererSlot(Gatherer gatherer)
        {
            // Check if already assigned
            if (assignedGatherers.Contains(gatherer))
            {
                Debug.LogWarning($"Gatherer {gatherer.name} already assigned to {gameObject.name}");
                return true;
            }

            // Check if slots available
            if (assignedGatherers.Count < maxGathererSlots)
            {
                assignedGatherers.Add(gatherer);
                return true;
            }

            return false;
        }

        public void ReleaseGathererSlot(Gatherer gatherer)
        {
            if (assignedGatherers.Contains(gatherer))
            {
                assignedGatherers.Remove(gatherer);
            }
            else
            {
                Debug.LogWarning(
                    $"Tried to release gatherer {gatherer.name} that wasn't assigned to {gameObject.name}"
                );
            }
        }

        public GathererType GetGathererType()
        {
            return gathererType;
        }

        public int GetCurrentGathererCount()
        {
            return assignedGatherers.Count;
        }

        public int GetAvailableSlots()
        {
            return maxGathererSlots - assignedGatherers.Count;
        }

        public bool IsGathererAssigned(Gatherer gatherer)
        {
            return assignedGatherers.Contains(gatherer);
        }

        // Optional: Get all assigned gatherers (useful for UI or debugging)
        public List<Gatherer> GetAssignedGatherers()
        {
            return new List<Gatherer>(assignedGatherers); // Return a copy to prevent external modification
        }
    }
}
