using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class BuildingData : MonoBehaviour
    {
        [Header("Basic Info")] public string buildingName;
        public string description;

        [Header("Costs")] public List<ResourceCost> costs;

        [Header("Refund")] public List<ResourceCost> refunds;

        public bool isMovable = true;
        public bool isDismantable = true;
    }
}