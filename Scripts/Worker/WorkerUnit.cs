using System.Collections;
using GameJam.Core;
using GameJam.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam.Worker
{
    public class WorkerUnit : MonoBehaviour
    {
        enum WorkerState
        {
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
        WorkerType initialWorkerType;

        [SerializeField]
        WorkCamp workCamp;

        private WorkerType currentWorkerType;
        private NavMeshAgent navMeshAgent;
        private Mover mover;
        private Health health;
        private Animator animator;
        private AudioSource audioSource;

        private WorkerState currentState;
        private ResourceNode targetNode;
        private GameObject instantiatedTool;
        private Coroutine gatherCoroutine;

        void Start()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            mover = GetComponent<Mover>();
            animator = GetComponent<Animator>();
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            //navMeshAgent tuning so there are less collisions between multiple workers
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            navMeshAgent.avoidancePriority = Random.Range(30, 60);
            //starting work cycle
            currentState = WorkerState.SearchingForResource;
            if (initialWorkerType != null)
            {
                SetWorkerType(initialWorkerType);
            }
        }

        void Update()
        {
            if (health.IsDead())
                return;
            if (currentWorkerType == null)
                return;

            switch (currentState)
            {
                case WorkerState.SearchingForResource:
                    SearchForResource();
                    break;
                case WorkerState.MovingToResource:
                    MoveToResource();
                    break;
                case WorkerState.GatheringResource:
                    GatherResource();
                    break;
                case WorkerState.ReturningResource:
                    ReturnResource();
                    break;
            }
        }

        //search for the nearest resource node of the correct type and try to reserve a slot for gathering - otherwise check next resource node
        private void SearchForResource()
        {
            var resourceObjects = GameObject.FindGameObjectsWithTag(
                currentWorkerType.GetResourceTag()
            );

            // Create a list of resource nodes with their distances
            var resourcesWithDistance = new System.Collections.Generic.List<(
                ResourceNode node,
                float distance
            )>();

            foreach (var resourceObj in resourceObjects)
            {
                ResourceNode node = resourceObj.GetComponent<ResourceNode>();
                if (node != null && node.CanGather(currentWorkerType.GetGatherAmount()))
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
                    currentState = WorkerState.MovingToResource;
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

                currentState = WorkerState.GatheringResource;
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
            navMeshAgent.isStopped = true;
            animator.SetTrigger("gather");
            yield return new WaitForSeconds(currentWorkerType.GetGatherTime());
            int gatherAmount = targetNode.Gather(currentWorkerType.GetGatherAmount());
            targetNode.ReleaseSlot(this);
            navMeshAgent.isStopped = false;
            animator.SetTrigger("stopGather");
            currentState = WorkerState.ReturningResource;
        }

        // event is triggered when the gather animation hits the resource
        void Hit()
        {
            audioSource.PlayOneShot(currentWorkerType.GetGatherSound());
        }

        private void ReturnResource()
        {
            gatherCoroutine = null; //clear gatherCoroutine
            mover.MoveTo(workCamp.transform.position);
            if (mover.IsDestinationReached())
            {
                workCamp.DepositResources(currentWorkerType.GetGatherAmount());
                currentState = WorkerState.SearchingForResource;
            }
        }

        //MMMMMMMMMMMMMMMMMMMMM
        // Call this method to change the current task
        public void SetWorkerType(WorkerType newWorkerType)
        {
            if (currentWorkerType == newWorkerType)
                return;

            if (newWorkerType == null)
                return;

            currentWorkerType = newWorkerType;
            instantiatedTool = Instantiate(newWorkerType.GetToolPrefab(), rightHandTransform);

            //TODO
        }
    }
}
