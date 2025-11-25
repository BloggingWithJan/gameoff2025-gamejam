using System.Collections;
using System.Collections.Generic;
using Core;
using GameJam.Military;
using GameJam.Movement;
using GameJam.Production;
using GameJam.Resource;
using Unity.VisualScripting;
using UnityEngine;

namespace GameJam.Core
{
    public class PlayerBaseBuilding : BaseBuilding
    {
        [SerializeField]
        GameObject unitPrefab;

        [SerializeField]
        float spawnInterval = 5f;

        [SerializeField]
        float initialDelay = 5f;

        private Coroutine repeatingSpawnCoroutine = null;

        private void Start()
        {
            // :D
        }

        public void SpawnUnit(ResourceCost cost)
        {
            if (Resource.ResourceManager.Instance.MaxPopulationReached())
                return;

            if (!ResourceManager.Instance.HasSufficientResources(cost))
            {
                Debug.Log("Not enough resources");
                return;
            }

            ResourceManager.Instance.DeductResources(cost);

            GameObject unit = Instantiate(unitPrefab, GetSpawnPoint(), Quaternion.identity);

            Mover mover = unit.GetComponent<Mover>();
            if (mover != null)
                mover.MoveToWithRandomOffset(GetSpawnPoint(), 5f);
        }

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
    }
}
