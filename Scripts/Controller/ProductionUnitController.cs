using GameJam.Combat;
using GameJam.Core;
using GameJam.Movement;
using GameJam.Production;
using UnityEngine;

namespace GameJam.Controller
{
    public class ProductionUnitController : MonoBehaviour, IUnitCommandable
    {
        [SerializeField]
        ProductionBuilding homeBuilding;

        [SerializeField]
        float autoCombatRange = 4f;

        private ActionScheduler actionScheduler;
        private Health health;
        private Gatherer gatherer;
        private Fighter fighter;

        void Start()
        {
            health = GetComponent<Health>();
            gatherer = GetComponent<Gatherer>();
            fighter = GetComponent<Fighter>();
            actionScheduler = GetComponent<ActionScheduler>();
            if (homeBuilding != null && gatherer != null)
            {
                gatherer.Gather(homeBuilding);
            }
        }

        void Update()
        {
            if (health.IsDead())
                return;

            //gathering should be interrupted by combat if an enemy comes close
            // if (actionScheduler.GetCurrentAction() is Gatherer && FindNearestEnemy() != null)
            // {
            //     AutoCombatBehavior();
            //     return;
            // }

            //if no current action is taken place, start automatic behaviors
            if (actionScheduler.GetCurrentAction() is null)
            {
                if (AutoCombatBehavior())
                    return;
            }
        }

        public void MoveTo(Vector3 destination)
        {
            GetComponent<Mover>().StartMoveAction(destination);
        }

        public void InteractWith(GameObject target)
        {
            if (target.GetComponent<ProductionBuilding>() != null)
            {
                ProductionBuilding building = target.GetComponent<ProductionBuilding>();
                if (gatherer != null)
                {
                    gatherer.Gather(building);
                }
            }
            if (target.tag == "Enemy")
            {
                Debug.Log("Attacking enemy " + target.name);
                GetComponent<Fighter>().Attack(target);
            }
        }

        private bool AutoCombatBehavior()
        {
            GameObject newTarget = FindNearestEnemy();
            if (newTarget != null)
            {
                fighter.Attack(newTarget);
                return true;
            }
            return false;
        }

        private GameObject FindNearestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject nearestEnemy = null;
            float shortestDistance = Mathf.Infinity;
            Vector3 currentPosition = transform.position;

            foreach (GameObject enemy in enemies)
            {
                if (enemy.GetComponent<Health>() == null || enemy.GetComponent<Health>().IsDead())
                    continue;

                float distanceToEnemy = Vector3.Distance(currentPosition, enemy.transform.position);

                // Only consider enemies within aggro range
                if (distanceToEnemy <= autoCombatRange && distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy;
                }
            }

            return nearestEnemy;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.pink;
            Gizmos.DrawWireSphere(transform.position, autoCombatRange);
        }
    }
}
