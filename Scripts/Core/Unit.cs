using Core;
using Data;
using GameJam.Combat;
using GameJam.Resource;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam.Core
{
    public class Unit : MonoBehaviour, IUnitData
    {
        [SerializeField]
        Transform rightHandTransform = null;

        [SerializeField]
        Transform leftHandTransform = null;

        [SerializeField]
        SkinnedMeshRenderer headSlotRenderer;

        [SerializeField]
        SkinnedMeshRenderer bodySlotRenderer;

        [SerializeField]
        string unitName = "Villager";
        private Weapon weapon = null;
        private Animator animator;
        private Health health;
        private NavMeshAgent navMeshAgent;
        private CapsuleCollider capsuleCollider;

        public BaseBuilding assignedBuilding;

        void Awake()
        {
            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            //navMeshAgent tuning so there are less collisions between multiple workers
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            navMeshAgent.avoidancePriority = Random.Range(30, 60);
            navMeshAgent.stoppingDistance = 0.5f;
            // navMeshAgent.avoidancePriority = 99; // lowest priority
            // navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            health.OnDeath += OnDeath;
        }

        void Start()
        {
            //add population only for player units
            if (gameObject.tag != "Enemy")
            {
                if (ResourceManager.Instance == null)
                {
                    Debug.LogError("ResourceManager instance is null. Cannot add population.");
                    return;
                }
                ResourceManager.Instance.AddPopulation(1);
            }
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

        public void SetUnitName(string newName)
        {
            unitName = newName;
        }

        public string GetUnitName()
        {
            return unitName;
        }

        private void OnDeath()
        {
            if (assignedBuilding != null)
            {
                assignedBuilding.ReleaseUnitSlot(this);
            }
            if (capsuleCollider != null)
            {
                capsuleCollider.enabled = false;
            }
            //decrease population only for player units
            if (gameObject.tag != "Enemy")
            {
                ResourceManager.Instance.DeductPopulation(1);
            }

            Destroy(gameObject, 2f);
        }

        public UnitData GetUnitData()
        {
            UnitData unitData = new UnitData();
            unitData.UnitName = unitName;
            return unitData;
        }
    }
}
