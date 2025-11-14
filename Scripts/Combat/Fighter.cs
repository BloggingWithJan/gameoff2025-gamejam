using GameJam.Core;
using GameJam.Movement;
using UnityEngine;

namespace GameJam.Combat
{
    public class Fighter : MonoBehaviour, IAction
    {
        [SerializeField]
        Weapon weapon = null;

        [SerializeField]
        float aggroRange = 5f;

        Health target;
        Health fighter;
        private Unit unit;
        float timeSinceLastAttack = Mathf.Infinity;

        private ActionScheduler actionScheduler;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            unit = GetComponent<Unit>();
            unit.SpawnWeapon(weapon);
            fighter = GetComponent<Health>();
            actionScheduler = GetComponent<ActionScheduler>();
        }

        // Update is called once per frame
        void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
        }

        public bool GetIsInRange()
        {
            return Vector3.Distance(transform.position, target.transform.position)
                < weapon.GetRange();
        }

        public void AttackBehaviour()
        {
            transform.LookAt(target.transform);
            if (timeSinceLastAttack > weapon.GetTimeBetweenAttacks())
            {
                GetComponent<Animator>().ResetTrigger("stopAttack");
                GetComponent<Animator>().SetTrigger("attack");
                timeSinceLastAttack = 0;
            }
        }

        void Hit()
        {
            if (target == null)
                return;
            target.TakeDamage(weapon.GetDamage());
        }

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null)
                return false;
            Health targetToTest = combatTarget.GetComponent<Health>();
            return targetToTest != null && !targetToTest.IsDead();
        }

        public void Attack(GameObject combatTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            target = combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopAttack");
            target = null;
        }

        public bool IsCurrentTargetDead()
        {
            if (target == null)
                return true;
            if (target.IsDead())
            {
                return true;
            }
            return false;
        }

        public Health ScanForTargetInRange()
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
                if (distanceToEnemy <= aggroRange && distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestTarget = enemyHealth;
                }
            }
            return nearestTarget;
        }

        public void SetCurrentTarget(Health newTarget)
        {
            target = newTarget;
        }

        public Health GetCurrentTarget()
        {
            return target;
        }

        public void MoveToCurrentTargetPosition()
        {
            if (target != null)
            {
                GetComponent<Mover>().MoveTo(target.transform.position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aggroRange);
        }
    }
}
