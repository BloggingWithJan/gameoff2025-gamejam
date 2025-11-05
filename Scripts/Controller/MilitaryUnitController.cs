using GameJam.Combat;
using GameJam.Core;
using UnityEngine;

namespace GameJam.Controller
{
    public class MilitaryUnitController : MonoBehaviour
    {
        [SerializeField]
        private float detectionRadius = 10f;
        private Health health;
        private Fighter fighter;

        private GameObject currenTarget;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            health = GetComponent<Health>();
            fighter = GetComponent<Fighter>();
        }

        // Update is called once per frame
        void Update()
        {
            if (health.IsDead())
                return;

            if (currenTarget == null)
            {
                ScanForEnemies();
            }

            if (currenTarget != null)
            {
                AttackBehaviour();
            }
        }

        private void ScanForEnemies()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance <= detectionRadius && distance < nearestDistance)
                {
                    // Check if enemy is alive
                    Health enemyHealth = enemy.GetComponent<Health>();
                    if (enemyHealth != null && !enemyHealth.IsDead())
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy;
                    }
                }
            }

            currenTarget = nearestEnemy;
        }

        // Gizmos to visualize detection radius
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        private void AttackBehaviour()
        {
            fighter.Attack(currenTarget);
        }
    }
}
