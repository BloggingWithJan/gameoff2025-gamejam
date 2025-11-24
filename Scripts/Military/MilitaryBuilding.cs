using System.Collections.Generic;
using Core;
using GameJam.Core;
using GameJam.Production;
using UnityEngine;

namespace GameJam.Military
{
    public class MilitaryBuilding : BaseBuilding
    {
        [SerializeField]
        private SoldierType soldierType;

        public override SoldierType GetSoldierType()
        {
            return soldierType;
        }

        public override GathererType GetGathererType()
        {
            Debug.LogError("GetGathererType called on MilitaryBuilding, which is invalid.");
            return null;
        }
    }
}
