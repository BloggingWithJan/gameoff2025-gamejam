using GameJam.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam.Core
{
    public class Unit : MonoBehaviour
    {
        [SerializeField]
        Transform rightHandTransform = null;

        [SerializeField]
        Transform leftHandTransform = null;

        [SerializeField]
        SkinnedMeshRenderer headSlotRenderer;

        [SerializeField]
        SkinnedMeshRenderer bodySlotRenderer;

        private Weapon weapon = null;
        private Animator animator;
        private NavMeshAgent navMeshAgent;

        public BaseBuilding assignedBuilding;

        void Start()
        {
            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            //navMeshAgent tuning so there are less collisions between multiple workers
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            navMeshAgent.avoidancePriority = Random.Range(30, 60);
            // navMeshAgent.avoidancePriority = 99; // lowest priority
            // navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        public void SpawnWeapon(Weapon weaponPrefab)
        {
            if (weapon != null)
            {
                weapon.Unequip();
            }
            if (weaponPrefab == null)
                return;
            weapon = weaponPrefab;
            weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        public Weapon GetWeapon()
        {
            return weapon;
        }

        public void ChangeMeshes(Mesh newHeadMesh, Mesh newBodyMesh)
        {
            headSlotRenderer.sharedMesh = newHeadMesh;
            bodySlotRenderer.sharedMesh = newBodyMesh;
        }

        public Transform GetRightHandTransform()
        {
            return rightHandTransform;
        }

        public Transform GetLeftHandTransform()
        {
            return leftHandTransform;
        }
    }
}
