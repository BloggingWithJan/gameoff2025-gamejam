using Core;
using GameJam.Military;
using GameJam.Production;
using GameJam.Resource;
using UnityEngine;

namespace GameJam.Core
{
    public class HouseBuilding : BaseBuilding
    {
        [SerializeField]
        private int populationIncrease = 5;

        public override GathererType GetGathererType()
        {
            Debug.LogError("GetGathererType called on HouseBuilding, which is invalid.");
            return null;
        }

        public override SoldierType GetSoldierType()
        {
            Debug.LogError("GetSoldierType called on HouseBuilding, which is invalid.");
            return null;
        }

        void Start()
        {
            Debug.Log("HouseBuilding Awake: Increasing max population by " + populationIncrease);
            ResourceManager.Instance.AddMaxPopulation(populationIncrease);
        }

        void OnDestroy()
        {
            ResourceManager.Instance.DeductMaxPopulation(populationIncrease);
        }
    }
}
