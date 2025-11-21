using System.Collections;
using GameJam.Military;
using GameJam.Movement;
using GameJam.Production;
using GameJam.Resource;
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

        private bool isCoroutineRunning = false;
        private Coroutine repeatingSpawnCoroutine = null;

        private void Start()
        {
            StartSpawningUnits();
        }

        void StartSpawningUnits()
        {
            if (repeatingSpawnCoroutine == null)
            {
                Debug.Log("Starting unit spawn coroutine.");
                repeatingSpawnCoroutine = StartCoroutine(SpawnUnit());
                isCoroutineRunning = true;
            }
        }

        IEnumerator SpawnUnit()
        {
            yield return new WaitForSeconds(initialDelay);
            // This loop makes it repeat forever
            while (true)
            {
                // PAUSE CHECK: The key to pausing/resuming
                while (Resource.ResourceManager.Instance.MaxPopulationReached())
                {
                    yield return null; // Wait one frame before checking the 'isPaused' condition again
                }
                GameObject unit = Instantiate(unitPrefab, GetSpawnPoint(), Quaternion.identity);
                Mover mover = unit.GetComponent<Mover>();

                yield return null; // Wait a frame to ensure the unit is fully initialized
                if (mover != null)
                {
                    mover.MoveToWithRandomOffset(GetSpawnPoint(), 5f);
                }

                // Wait for the specified time before repeating
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // 3. Pause the Coroutine
        void PauseRepeatingAction()
        {
            isCoroutineRunning = false;
        }

        // 4. Resume the Coroutine
        void ResumeRepeatingAction()
        {
            isCoroutineRunning = true;
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
