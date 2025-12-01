using Core;
using GameJam.Core;
using GameJam.Movement;
using UnityEngine;

namespace GameJam.Combat
{
    public class Fighter : MonoBehaviour, IAction
    {
        [SerializeField]
        float aggroRange = 5f;

        Health target;
        Health fighter;
        private Unit unit;
        float timeSinceLastAttack = Mathf.Infinity;

        private ActionScheduler actionScheduler;
        private AudioSource audioSource;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            unit = GetComponent<Unit>();
            fighter = GetComponent<Health>();
            actionScheduler = GetComponent<ActionScheduler>();
            audioSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
        }

        public bool GetIsInRange()
        {
            if (target == null)
                return false;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            float effectiveRange = unit.GetWeapon().GetRange();

            // If target is a building, add its collider size to the range
            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider != null)
            {
                // Get the closest point on the collider bounds to the unit
                Vector3 closestPoint = targetCollider.ClosestPoint(transform.position);
                distance = Vector3.Distance(transform.position, closestPoint);
            }

            return distance < effectiveRange;
        }

        public void AttackBehaviour()
        {
            transform.LookAt(target.transform);
            if (timeSinceLastAttack > unit.GetWeapon().GetTimeBetweenAttacks())
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
            target.TakeDamage(unit.GetWeapon().GetDamage());
            audioSource.PlayOneShot(unit.GetWeapon().GetAttackSound());
        }

        void Shoot()
        {
            if (target == null)
                return;
            unit.GetWeapon()
                .LaunchProjectile(
                    unit.GetRightHandTransform(),
                    unit.GetLeftHandTransform(),
                    target
                );
            audioSource.PlayOneShot(unit.GetWeapon().GetAttackSound());
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
