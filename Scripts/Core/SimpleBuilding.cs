using Core;
using GameJam.Military;
using GameJam.Production;
using UnityEngine;

namespace GameJam.Core
{
    public class SimpleBuilding : BaseBuilding
    {
        public override GathererType GetGathererType()
        {
            Debug.LogError("GetGathererType called on SimpleBuilding, which is invalid.");
            return null;
        }

        public override SoldierType GetSoldierType()
        {
            Debug.LogError("GetSoldierType called on SimpleBuilding, which is invalid.");
            return null;
        }
    }
}
