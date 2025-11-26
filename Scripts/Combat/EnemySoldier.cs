using System;
using Core;
using Data;
using GameJam.Core;
using GameJam.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam.Combat
{
    public class EnemySoldier : MonoBehaviour, IAction
    {
        public enum EnemyState
        {
            Idle,
            MovingToBase, // Default: pathfinding to player base
            EngagingDefender, // Combat with units
            AttackingBuilding, // Attacking a building
        }

        [SerializeField]
        Weapon weapon;

        [Header("Target Settings")]
        [SerializeField]
        private Transform playerBase; // Set by spawner

        [SerializeField]
        private float detectionRadius = 15f;

        [SerializeField]
        private float baseDetectionRadius = 8f; // When to attack buildings

        [Header("Behavior")]
        [SerializeField]
        private float updateTargetInterval = 0.5f; // Check for new targets every 0.5s

        public EnemyState currentState;
        private Health health;
        private Fighter fighter;
        private Mover mover;
        private NavMeshAgent navMeshAgent;

        private Unit unit;
        private float lastTargetUpdateTime = 0f;

        void Awake()
        {
            health = GetComponent<Health>();
            fighter = GetComponent<Fighter>();
            mover = GetComponent<Mover>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            unit = GetComponent<Unit>();
        }

        public void Slay()
        {
            GetComponent<ActionScheduler>().StartAction(this);
            // If no base assigned, try to find it
            if (playerBase == null)
            {
                GameObject baseObj = GameObject.FindGameObjectWithTag("PlayerBase");
                if (baseObj != null)
                    playerBase = baseObj.transform;
            }

            unit.SpawnWeapon(weapon);

            currentState = EnemyState.MovingToBase;
        }

        public void Cancel()
        {
            fighter.Cancel();
            navMeshAgent.isStopped = true;
            currentState = EnemyState.Idle;
        }

        void Update()
        {
            if (health.IsDead())
                return;

            // Periodically check for new targets (optimization)
            if (Time.time - lastTargetUpdateTime > updateTargetInterval)
            {
                UpdateTargetPriority();
                lastTargetUpdateTime = Time.time;
            }

            // Execute behavior based on state
            switch (currentState)
            {
                case EnemyState.MovingToBase:
                    MoveToBase();
                    break;
                case EnemyState.EngagingDefender:
                    EngageDefender();
                    break;
                // case EnemyState.AttackingBuilding:
                //     AttackBuilding();
                // break;
            }
        }

        private void UpdateTargetPriority()
        {
            // Priority 1: Defenders attacking me or nearby
            Health nearbyDefender = FindNearestThreat();
            if (nearbyDefender != null)
            {
                SwitchToDefenderCombat(nearbyDefender);
                return;
            }

            // Priority 2: Buildings in range (when close to base)
            Health nearbyBuilding = FindNearestBuilding();
            if (nearbyBuilding != null)
            {
                SwitchToDefenderCombat(nearbyBuilding);
                return;
            }

            // Priority 3: Continue to base
            if (currentState != EnemyState.MovingToBase)
            {
                currentState = EnemyState.MovingToBase;
                fighter.SetCurrentTarget(null);
            }
        }

        private void MoveToBase()
        {
            if (playerBase == null)
            {
                Debug.LogWarning("Enemy has no base target!");
                return;
            }

            // Navigate toward base
            if (mover != null)
            {
                mover.MoveTo(playerBase.GetComponent<BaseBuilding>().GetSpawnPoint());
            }
        }

        private void EngageDefender()
        {
            float distance = Vector3.Distance(
                transform.position,
                fighter.GetCurrentTarget().transform.position
            );
            if (
                fighter.GetCurrentTarget() == null
                || fighter.GetCurrentTarget().IsDead()
                || distance > detectionRadius * 1.5f
            )
            {
                navMeshAgent.isStopped = false;
                currentState = EnemyState.MovingToBase;
                fighter.SetCurrentTarget(null);
                return;
            }

            if (!fighter.GetIsInRange())
            {
                navMeshAgent.isStopped = false;
                fighter.MoveToCurrentTargetPosition();
            }
            else
            {
                navMeshAgent.isStopped = true;
                fighter.AttackBehaviour();
            }
        }

        // private void AttackBuilding()
        // {
        //     if (currentTarget == null || IsTargetDead(currentTarget))
        //     {
        //         currentState = EnemyState.MovingToBase;
        //         currentTarget = null;
        //         return;
        //     }

        //     // Attack the building
        //     fighter.Attack(currentTarget);
        // }

        private Health FindNearestThreat()
        {
            // Find defenders (units with Fighter component, not enemies)
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            Health nearestDefender = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider col in colliders)
            {
                // Skip self and other enemies
                if (col.gameObject == gameObject || col.CompareTag("Enemy"))
                    continue;

                // Check if it's a military unit or production unit with combat capability
                Unit defenderUnit = col.GetComponent<Unit>();
                Fighter defenderFighter = col.GetComponent<Fighter>();
                Health defenderHealth = col.GetComponent<Health>();

                if (
                    defenderUnit != null
                    && defenderFighter != null
                    && defenderHealth != null
                    && !defenderHealth.IsDead()
                )
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);

                    // Prioritize units that are attacking this enemy
                    if (defenderFighter.GetCurrentTarget()?.gameObject == gameObject)
                    {
                        return defenderHealth; // Immediately switch to attacker
                    }

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestDefender = defenderHealth;
                    }
                }
            }

            return nearestDefender;
        }

        private Health FindNearestBuilding()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, baseDetectionRadius);
            Health nearestBuilding = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider col in colliders)
            {
                if (col.gameObject == gameObject || col.CompareTag("Enemy"))
                    continue;

                // Check if it's a building (has Health but no Fighter - or specific Building component)
                Health buildingHealth = col.GetComponent<Health>();
                bool isBuilding = col.GetComponent<BaseBuilding>() != null;
                // bool isBuilding = col.GetComponent<GameJam.Production.BaseBuilding>() != null;

                if (isBuilding && buildingHealth != null && !buildingHealth.IsDead())
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestBuilding = buildingHealth;
                    }
                }
            }

            return nearestBuilding;
        }

        private void SwitchToDefenderCombat(Health defender)
        {
            if (
                currentState != EnemyState.EngagingDefender
                || fighter.GetCurrentTarget() != defender
            )
            {
                currentState = EnemyState.EngagingDefender;
                fighter.SetCurrentTarget(defender);
            }
        }

        private void SwitchToBuildingAttack(Health building)
        {
            if (
                currentState != EnemyState.AttackingBuilding
                || fighter.GetCurrentTarget() != building
            )
            {
                currentState = EnemyState.AttackingBuilding;
                fighter.SetCurrentTarget(building);
            }
        }

        // Visualization in Scene view
        private void OnDrawGizmosSelected()
        {
            // Detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Building detection radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, baseDetectionRadius);

            // Line to base
            if (playerBase != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, playerBase.position);
            }

            // // Line to current target
            // if (fighter.GetCurrentTarget() != null)
            // {
            //     Gizmos.color = Color.red;
            //     Gizmos.DrawLine(transform.position, fighter.GetCurrentTarget().transform.position);
            // }
        }

        public void SetPlayerBase(Transform baseTransform)
        {
            playerBase = baseTransform;
        }
    }
}
