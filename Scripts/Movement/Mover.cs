using Core;
using GameJam.Core;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam.Movement
{
    public class Mover : MonoBehaviour, IAction
    {
        private NavMeshAgent agent;
        private Animator animator;
        private ActionScheduler actionScheduler;
        private Health health;

        private Vector3 commandedDestination;
        private Vector3 formationSlot;

        [SerializeField]
        private float arrivalThreshold = 0.5f; // Distance threshold to consider as "

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            actionScheduler = GetComponent<ActionScheduler>();
            health = GetComponent<Health>();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateAnimator();

            //check if destination is reached to stop the agent
            if (IsDestinationReached())
            {
                actionScheduler.CancelIfCurrentActionIs(this);
            }
        }

        public void Cancel()
        {
            agent.isStopped = true;
        }

        public void StartMoveAction(Vector3 destination, Vector3 formationPosition)
        {
            GetComponent<ActionScheduler>().StartAction(this);

            commandedDestination = destination;
            formationSlot = formationPosition;

            agent.SetDestination(formationSlot);
            agent.isStopped = false;
        }

        public void MoveTo(Vector3 destination)
        {
            formationSlot = destination;
            agent.destination = destination;
            agent.isStopped = false;
        }

        public void MoveToFormationSlot(Vector3 destination, Vector3 formationPosition)
        {
            commandedDestination = destination;
            formationSlot = formationPosition;

            agent.SetDestination(formationSlot);
            agent.isStopped = false;
        }

        public Vector3 GetSingleFormationPosition(Vector3 destination)
        {
            // Single unit - move to clicked position with small offset
            Vector3 offset = UnityEngine.Random.insideUnitSphere * 0.5f;
            offset.y = 0;
            return destination + offset;
        }

        public void MoveToWithRandomOffset(Vector3 destination, float offsetRadius)
        {
            Vector3 randomOffset = Random.insideUnitSphere * offsetRadius; // 1.5f is the radius of the offset
            randomOffset.y = 0; // Keep the offset on the horizontal plane
            Vector3 adjustedDestination = destination + randomOffset;
            MoveTo(adjustedDestination);
        }

        // public bool IsDestinationReached()
        // {
        //     if (!agent.pathPending)
        //     {
        //         if (agent.remainingDistance <= agent.stoppingDistance)
        //         {
        //             if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
        //             {
        //                 return true;
        //             }
        //         }
        //     }
        //     return false;
        // }

        public bool IsDestinationReached()
        {
            if (agent.pathPending)
                return false;

            // Option 1: Check if close to formation slot
            float distToSlot = Vector3.Distance(transform.position, formationSlot);
            if (distToSlot <= arrivalThreshold)
                return true;

            // Option 2: Check if NavMesh thinks we arrived
            if (agent.remainingDistance <= agent.stoppingDistance + 0.5f)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                    return true;
            }

            return false;
        }

        private void UpdateAnimator()
        {
            Vector3 velocity = agent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;
            animator.SetFloat("forwardSpeed", speed);
        }
    }
}
