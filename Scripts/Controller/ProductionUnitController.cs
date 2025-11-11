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
        float enemyAggroRange = 5f;

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

            //TODO cancels movement actions right now when an enemy is in range
            //may use ActionScheduler better - not only start an action also end/cancel an action
            GameObject newTarget = FindNearestEnemy();
            if (newTarget != null)
            {
                if (fighter.IsCurrentTargetDead())
                    fighter.Attack(newTarget);
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

            // currentMode = UnitMode.Manual;

            // if (target.GetComponent<WorkCamp>() != null) { }

            // GetComponent<Mover>().StartMoveAction(target.transform.position);
            //workertype ändern sobald die destination erreicht wurde

            //einfach prüfen ob  target die componente oder die hat - je nachdem
            //je nachdem was für ein gameObject
            //entweder gatheren
            //oder angreifen
            //...
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
                if (distanceToEnemy <= enemyAggroRange && distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy;
                }
            }

            return nearestEnemy;
        }
    }
}
