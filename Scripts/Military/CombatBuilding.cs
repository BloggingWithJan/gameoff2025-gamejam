using GameJam.Combat;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Military
{
    public class CombatBuilding : MonoBehaviour
    {
        public enum CombatBuildingState
        {
            Idle,
            Patrol,
            AutomaticCombat,
        }

        [SerializeField]
        Weapon weapon;

        public CombatBuildingState currentState;

        private Health health;
        private Health currentTarget;

        private AudioSource audioSource;

        private float timeSinceLastAttack = Mathf.Infinity;

        void Start()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            Patrol();
        }

        void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            if (health.IsDead())
                return;
            if (currentState == CombatBuildingState.Idle)
                return;

            switch (currentState)
            {
                case CombatBuildingState.Patrol:
                    AutomaticPatrol();
                    break;
                case CombatBuildingState.AutomaticCombat:
                    AutomaticCombat();
                    break;
            }
        }

        public void Patrol()
        {
            currentState = CombatBuildingState.Patrol;
        }

        private void AutomaticPatrol()
        {
            Health targetInRange = ScanForTargetInRange();
            if (targetInRange != null)
            {
                currentTarget = targetInRange;
                currentState = CombatBuildingState.AutomaticCombat;
            }
        }

        private void AutomaticCombat()
        {
            if (currentTarget == null || currentTarget.IsDead())
            {
                currentState = CombatBuildingState.Patrol;
                return;
            }
            if (GetIsInRange())
            {
                AttackBehaviour();
            }
        }

        private void AttackBehaviour()
        {
            if (timeSinceLastAttack > weapon.GetTimeBetweenAttacks())
            {
                timeSinceLastAttack = 0;
                weapon.LaunchProjectile(transform, transform, currentTarget);
                audioSource.PlayOneShot(weapon.GetAttackSound());
            }
        }

        private bool GetIsInRange()
        {
            return Vector3.Distance(transform.position, currentTarget.transform.position)
                < weapon.GetRange();
        }

        private Health ScanForTargetInRange()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Health nearestTarget = null;
            float shortestDistance = Mathf.Infinity;
            Vector3 currentPosition = transform.position;

            foreach (GameObject enemy in enemies)
            {
                Health enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth == null || enemyHealth.IsDead())
                    continue;

                float distanceToEnemy = Vector3.Distance(currentPosition, enemy.transform.position);
                if (distanceToEnemy <= weapon.GetRange() && distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestTarget = enemyHealth;
                }
            }
            return nearestTarget;
        }
    }
}
