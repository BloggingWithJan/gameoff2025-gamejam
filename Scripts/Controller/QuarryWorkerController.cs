using GameJam.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam.Controller
{
    public class SeekerController : MonoBehaviour
    {
        enum SeekerState
        {
            Searching,
            MovingToDestination,
            DoingSomething,
            ReturningToSpawn,
        }

        private SeekerState currentState = SeekerState.Searching;
        private NavMeshAgent agent;
        private Animator animator;
        private Mover mover;
        private Vector3 spawnPosition;
        private GameObject targetResource;

        [Header("Debug Path")] [SerializeField]
        private bool showPath = true;
        [SerializeField] private Color pathColor = Color.red;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            mover = GetComponent<Mover>();
            spawnPosition = transform.position;
        }

        void Update()
        {
            switch (currentState)
            {
                case SeekerState.Searching:
                    Search();
                    break;
                case SeekerState.MovingToDestination:
                    MoveToDestination();
                    break;
                case SeekerState.DoingSomething:
                    DoingSomething();
                    break;
                case SeekerState.ReturningToSpawn:
                    ReturningToSpawn();
                    break;
            }
        }

        void Search()
        {
            var resources = GameObject.FindGameObjectsWithTag("Rock");
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

            if (targetResource)
                currentState = SeekerState.MovingToDestination;
        }

        //Move to the targeted resource
        void MoveToDestination()
        {
            if (targetResource == null)
            {
                Debug.LogError("No target resource found. Going back to SeekerState.Searching");
                currentState = SeekerState.Searching;
                return;
            }

            mover.MoveTo(targetResource.transform.position);
            if (mover.IsDestinationReached()) currentState = SeekerState.DoingSomething;
        }

        void ReturningToSpawn()
        {
            mover.MoveTo(spawnPosition);
            if (mover.IsDestinationReached())
                //TODO: Animation "puff"
                Destroy(gameObject);
        }

        void DoingSomething()
        {
            currentState = SeekerState.ReturningToSpawn;
        }

        /**
         * For Debugging purposes.
         * Draw the path that the QuarryWorker will use.
         */
        void OnDrawGizmos() 
        {
            if (!showPath)
                return;

            if (agent == null)
            {
                Debug.LogError("No agent assigned to SeekerController");
                return;
            }

            var path = agent.path;
            if (path == null || path.corners == null || path.corners.Length < 2)
                return;

            Gizmos.color = pathColor;
            var corners = path.corners;

            for (int i = 0; i < corners.Length - 1; i++)
                Gizmos.DrawLine(corners[i], corners[i + 1]);
        }
    }
}