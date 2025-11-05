using System.Collections;
using GameJam.Core;
using GameJam.Movement;
using GameJam.Resource;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam.Worker
{
    public class Gatherer : MonoBehaviour, IAction
    {
        public enum GathererState
        {
            Idle,
            SearchingForResource,
            MovingToResource,
            GatheringResource,
            ReturningResource,
        }

        [SerializeField]
        Transform rightHandTransform;

        [SerializeField]
        Transform leftHandTransform;

        [SerializeField]
        GathererType initialGathererType;

        [SerializeField]
        WorkCamp workCamp;

        public GathererState currentState;

        private GathererType currentGathererType;
        private NavMeshAgent navMeshAgent;
        private Mover mover;
        private Health health;
        private Animator animator;
        private AudioSource audioSource;
        private ActionScheduler actionScheduler;

        private ResourceNode targetNode;
        private GameObject instantiatedTool;
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
            //navMeshAgent tuning so there are less collisions between multiple workers
            // navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            // navMeshAgent.avoidancePriority = Random.Range(30, 60);
            navMeshAgent.avoidancePriority = 99; // lowest priority
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            //starting work cycle
            if (initialGathererType != null)
            {
                SetGathererType(initialGathererType);
            }
        }

        void Update()
        {
            if (health.IsDead())
                return;
            if (currentGathererType == null)
                return;
            if (currentState == GathererState.Idle)
                return;

            switch (currentState)
            {
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

            // No available slots found in any resource
            targetNode = null;
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
                gatheredAmount = targetNode.Gather(currentGathererType.GetGatherAmount());
                targetNode.ReleaseSlot(this);
                targetNode.OnResourceDepleted -= HandleTargetNodeDepleted;
                targetNode = null;
            }
            animator.SetTrigger("stopGather");
            currentState = GathererState.ReturningResource;
        }

        // event is triggered when the gather animation hits the resource
        void Hit()
        {
            audioSource.PlayOneShot(currentGathererType.GetGatherSound());
        }

        private void ReturnResource()
        {
            gatherCoroutine = null; //clear gatherCoroutine
            mover.MoveTo(workCamp.transform.position);
            if (mover.IsDestinationReached())
            {
                workCamp.DepositResources(gatheredAmount);
                gatheredAmount = 0;
                currentState = GathererState.SearchingForResource;
            }
        }

        private void HandleTargetNodeDepleted()
        {
            Cancel();
            currentState = GathererState.SearchingForResource;
        }

        public void Gather()
        {
            actionScheduler.StartAction(this);
            if (currentState == GathererState.Idle)
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

        //MMMMMMMMMMMMMMMMMMMMM
        // Call this method to change the current task
        public void SetGathererType(GathererType newGathererType)
        {
            if (currentGathererType == newGathererType)
                return;

            if (newGathererType == null)
                return;

            if (instantiatedTool != null)
                Destroy(instantiatedTool);

            currentGathererType = newGathererType;
            instantiatedTool = Instantiate(newGathererType.GetToolPrefab(), rightHandTransform);
        }
    }
}
