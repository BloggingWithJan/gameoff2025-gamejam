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
            var resources = GameObject.FindGameObjectsWithTag("Resource");
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
            if (targetResource != null)
                mover.MoveTo(targetResource.transform.position);

            if (mover.IsDestinationReached())
                currentState = SeekerState.DoingSomething;
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
    }
}
