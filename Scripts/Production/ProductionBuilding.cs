using System.Collections.Generic;
using Core;
using GameJam.Core;
using GameJam.Military;
using GameJam.Production;
using GameJam.Resource;
using UnityEngine;

namespace Production
{
    public class ProductionBuilding : BaseBuilding
    {
        [SerializeField]
        private GathererType gathererType;

        public void DepositResources(int amount)
        {
            if (gathererType.GetResourceTag() == "Stone")
                ResourceManager.Instance.AddResource(ResourceType.Stone, amount);
            else if (gathererType.GetResourceTag() == "Wood")
                ResourceManager.Instance.AddResource(ResourceType.Wood, amount);
            else if (gathererType.GetResourceTag() == "Food")
                ResourceManager.Instance.AddResource(ResourceType.Food, amount);
        }

        public override GathererType GetGathererType()
        {
            return gathererType;
        }

        public override SoldierType GetSoldierType()
        {
            Debug.LogError("GetSoldierType called on ProductionBuilding, which is invalid.");
            return null;
        }
    }
}
