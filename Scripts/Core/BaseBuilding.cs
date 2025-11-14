using System.Collections.Generic;
using GameJam.Military;
using GameJam.Production;
using UnityEngine;

namespace GameJam.Core
{
    public abstract class BaseBuilding : MonoBehaviour
    {
        public enum BuildingType
        {
            Military,
            Production,
        }

        [SerializeField]
        private string buildingName;

        [SerializeField]
        private string description;

        [SerializeField]
        private BuildingType buildingType;

        [SerializeField]
        private int maxUnitSlots = 3;

        private List<Unit> assignedUnits = new List<Unit>();

        public bool RequestUnitSlot(Unit unit)
        {
            // Check if already assigned
            if (assignedUnits.Contains(unit))
            {
                Debug.LogWarning($"Unit {unit.name} already assigned to {gameObject.name}");
                return true;
            }

            // Check if slots available
            if (assignedUnits.Count < maxUnitSlots)
            {
                assignedUnits.Add(unit);
                return true;
            }

            return false;
        }

        public void ReleaseUnitSlot(Unit unit)
        {
            if (assignedUnits.Contains(unit))
            {
                assignedUnits.Remove(unit);
            }
            else
            {
                Debug.LogWarning(
                    $"Tried to release unit {unit.name} that wasn't assigned to {gameObject.name}"
                );
            }
        }

        public abstract SoldierType GetSoldierType();
        public abstract GathererType GetGathererType();
    }
}
