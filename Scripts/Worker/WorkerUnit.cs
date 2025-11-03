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

        private WorkerType currentWorkerType;
        private NavMeshAgent navMeshAgent;
        private Mover mover;
        private Health health;
        private Animator animator;

        private WorkerState currentState;
        private GameObject targetResource;
        private Vector3 workLocation;
        private GameObject instantiatedTool;

        void Start()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            mover = GetComponent<Mover>();
            animator = GetComponent<Animator>();
            health = GetComponent<Health>();
            currentState = WorkerState.SearchingForResource;
            workLocation = transform.position;
            if (initialWorkerType != null)
            {
                SetWorkerType(initialWorkerType);
            }
        }

        // Update is called once per frame
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

        private void SearchForResource()
        {
            var resources = GameObject.FindGameObjectsWithTag(currentWorkerType.GetResourceTag());
            float closestDist = Mathf.Infinity;
            foreach (var resource in resources)
            {
                float dist = Vector3.Distance(transform.position, resource.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetResource = resource;
                }
            }
            if (targetResource != null)
            {
                currentState = WorkerState.MovingToResource;
            }
        }

        private void MoveToResource()
        {
            //no target resource - go back to searching
            if (targetResource == null)
            {
                currentState = WorkerState.SearchingForResource;
                return;
            }

            mover.MoveTo(targetResource.transform.position);
            if (mover.IsDestinationReached())
                currentState = WorkerState.GatheringResource;
        }

        private void GatherResource()
        {
            currentState = WorkerState.ReturningResource;
        }

        private void ReturnResource()
        {
            mover.MoveTo(workLocation);
            if (mover.IsDestinationReached())
                currentState = WorkerState.SearchingForResource;
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
