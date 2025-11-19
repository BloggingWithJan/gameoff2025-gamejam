using System.Collections;
using Core;
using Data;
using GameJam.Combat;
using GameJam.Core;
using GameJam.Movement;
using GameJam.Production;
using GameJam.Resource;
using UnityEngine;
using UnityEngine.AI;

namespace Production
{
    public class Gatherer : MonoBehaviour, IAction, IUnitData
    {
        public enum GathererState
        {
            Idle,
            MoveToProductionBuilding,
            SearchingForResource,
            MovingToResource,
            GatheringResource,
            ReturningResource,
        }

        public GathererState currentState;

        private GathererType currentGathererType;
        private NavMeshAgent navMeshAgent;
        private Mover mover;
        private Health health;
        private Animator animator;
        private AudioSource audioSource;
        private ActionScheduler actionScheduler;
        private Unit unit;

        private ResourceNode targetNode;
        private Coroutine gatherCoroutine;

        private int gatheredAmount;

        void Start()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            mover = GetComponent<Mover>();
            animator = GetComponent<Animator>();
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            actionScheduler = GetComponent<ActionScheduler>();
            unit = GetComponent<Unit>();
        }

        void Update()
        {
            if (health.IsDead())
                return;
            if (currentState == GathererState.Idle)
                return;

            switch (currentState)
            {
                case GathererState.MoveToProductionBuilding:
                    MoveToProductionBuilding();
                    break;
                case GathererState.SearchingForResource:
                    SearchForResource();
                    break;
                case GathererState.MovingToResource:
                    MoveToResource();
                    break;
                case GathererState.GatheringResource:
                    GatherResource();
                    break;
                case GathererState.ReturningResource:
                    ReturnResource();
                    break;
            }
        }

        public void Gather(ProductionBuilding building)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            if (unit.assignedBuilding == building)
            {
                currentState = GathererState.MoveToProductionBuilding;
                return;
            }

            if (building.RequestUnitSlot(unit))
            {
                if (unit.assignedBuilding != null)
                {
                    unit.assignedBuilding.ReleaseUnitSlot(unit);
                    unit.assignedBuilding.OnBuildingDestroyed -= HandleAssignedBuildingDestroyed;
                }
                unit.assignedBuilding = building;
                unit.assignedBuilding.OnBuildingDestroyed += HandleAssignedBuildingDestroyed;

                currentState = GathererState.MoveToProductionBuilding;
            }
        }

        private void MoveToProductionBuilding()
        {
            Vector3 destination = unit.assignedBuilding.GetSpawnPoint();
            mover.MoveToFormationSlot(destination, mover.GetSingleFormationPosition(destination));
            if (mover.IsDestinationReached())
            {
                Debug.Log("Reached production building " + unit.assignedBuilding.name);
                SetGathererType(unit.assignedBuilding.GetGathererType());
            }
        }

        //search for the nearest resource node of the correct type and try to reserve a slot for gathering - otherwise check next resource node
        private void SearchForResource()
        {
            var resourceObjects = GameObject.FindGameObjectsWithTag(
                currentGathererType.GetResourceTag()
            );

            // Create a list of resource nodes with their distances
            var resourcesWithDistance = new System.Collections.Generic.List<(
                ResourceNode node,
                float distance
            )>();

            foreach (var resourceObj in resourceObjects)
            {
                ResourceNode node = resourceObj.GetComponent<ResourceNode>();
                if (node != null && node.CanGather(currentGathererType.GetGatherAmount()))
                {
                    float distance = Vector3.Distance(
                        transform.position,
                        resourceObj.transform.position
                    );
                    resourcesWithDistance.Add((node, distance));
                }
            }

            // Sort by distance (closest first)
            resourcesWithDistance.Sort((a, b) => a.distance.CompareTo(b.distance));

            // Try to reserve a slot, starting with the closest resource
            foreach (var (node, _) in resourcesWithDistance)
            {
                if (node.TryReserveSlot(this))
                {
                    // Successfully reserved a slot
                    targetNode = node;
                    targetNode.OnResourceDepleted += HandleTargetNodeDepleted;
                    currentState = GathererState.MovingToResource;
                    return;
                }
            }

            // No available slots found in any resource - go back to idle
            targetNode = null;
            actionScheduler.CancelCurrentAction();
        }

        //move to the reserved slot at the resource node - switch to gathering state when reached
        private void MoveToResource()
        {
            mover.MoveTo(targetNode.GetSlotPosition(this));
            if (mover.IsDestinationReached())
            {
                // Face towards the resource center
                Vector3 facingDirection = targetNode.GetFacingDirection(this);
                if (facingDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(facingDirection);
                }

                currentState = GathererState.GatheringResource;
            }
        }

        private void GatherResource()
        {
            if (gatherCoroutine != null)
                return;
            gatherCoroutine = StartCoroutine(GatherLoop());
        }

        private IEnumerator GatherLoop()
        {
            animator.SetTrigger("gather");
            yield return new WaitForSeconds(currentGathererType.GetGatherTime());
            if (targetNode != null)
            {
                try
                {
                    gatheredAmount = targetNode.Gather(currentGathererType.GetGatherAmount());
                    targetNode.ReleaseSlot(this);
                    targetNode.OnResourceDepleted -= HandleTargetNodeDepleted;
                    targetNode = null;
                }
                catch
                {
                    gatheredAmount = 0;
                }
            }
            animator.SetTrigger("stopGather");
            currentState = GathererState.ReturningResource;
        }

        // event is triggered when the gather animation hits the resource
        private void GatherAnimationEvent()
        {
            audioSource.PlayOneShot(currentGathererType.GetGatherSound());
        }

        private void ReturnResource()
        {
            gatherCoroutine = null; //clear gatherCoroutine
            Vector3 destination = unit.assignedBuilding.GetSpawnPoint();
            mover.MoveToFormationSlot(destination, mover.GetSingleFormationPosition(destination));
            if (mover.IsDestinationReached())
            {
                if (unit.assignedBuilding is ProductionBuilding productionBuilding)
                {
                    productionBuilding.DepositResources(gatheredAmount);
                }
                gatheredAmount = 0;
                currentState = GathererState.SearchingForResource;
            }
        }

        private void HandleTargetNodeDepleted()
        {
            Cancel();
            currentState = GathererState.SearchingForResource;
        }

        public void Cancel()
        {
            if (gatherCoroutine != null)
            {
                StopCoroutine(gatherCoroutine);
                gatherCoroutine = null;
                animator.SetTrigger("stopGather");
                gatheredAmount = 0;
            }

            if (targetNode != null)
            {
                targetNode.ReleaseSlot(this);
                targetNode.OnResourceDepleted -= HandleTargetNodeDepleted;
                targetNode = null;
            }

            navMeshAgent.isStopped = true;
            currentState = GathererState.Idle;
        }

        private void SetGathererType(GathererType newGathererType)
        {
            if (currentGathererType == newGathererType)
            {
                currentState = GathererState.SearchingForResource;
                return;
            }

            if (newGathererType == null)
                return;

            currentGathererType = newGathererType;

            unit.ChangeMeshes(newGathererType.GetHeadMesh(), newGathererType.GetBodyMesh());
            unit.SpawnWeapon(newGathererType.GetToolPrefab());

            currentState = GathererState.SearchingForResource;
        }

        //TODO
        public UnitData GetUnitData()
        {
            UnitData unitData = new UnitData();
            unitData.UnitName = unit.name;
            unitData.Status = currentState.ToString();

            return unitData;
        }

        private void HandleAssignedBuildingDestroyed()
        {
            actionScheduler.CancelIfCurrentActionIs(this);
            unit.assignedBuilding = null;
        }
    }
}
