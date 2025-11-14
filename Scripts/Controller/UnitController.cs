using GameJam.Combat;
using GameJam.Core;
using GameJam.Military;
using GameJam.Movement;
using Production;
using UnityEngine;

namespace GameJam.Controller
{
    public class UnitController : MonoBehaviour, IUnitCommandable
    {
        [SerializeField]
        ProductionBuilding homeBuilding;

        [SerializeField]
        float autoCombatRange = 4f;

        private ActionScheduler actionScheduler;
        private Health health;
        private Gatherer gatherer;
        private Fighter fighter;
        private Mover mover;
        private Soldier soldier;
        private Unit unit;

        private float timeSinceCurrentActionIsNull = 0f;

        public SkinnedMeshRenderer headSlotRenderer;
        public SkinnedMeshRenderer bodySlotRenderer;

        void Start()
        {
            health = GetComponent<Health>();
            gatherer = GetComponent<Gatherer>();
            fighter = GetComponent<Fighter>();
            actionScheduler = GetComponent<ActionScheduler>();
            mover = GetComponent<Mover>();
            soldier = GetComponent<Soldier>();
            unit = GetComponent<Unit>();
            if (homeBuilding != null && gatherer != null)
            {
                gatherer.Gather(homeBuilding);
            }
        }

        void Update()
        {
            if (health.IsDead())
                return;

            //if no current action is taken place, start automatic behaviors
            if (actionScheduler.GetCurrentAction() is null)
            {
                timeSinceCurrentActionIsNull += Time.deltaTime;
                if (AutoCombatBehavior())
                    return;
                if (AutoGathererBehavior())
                    return;
            }
            else
            {
                timeSinceCurrentActionIsNull = 0f;
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
            if (target.GetComponent<MilitaryBuilding>() != null)
            {
                MilitaryBuilding building = target.GetComponent<MilitaryBuilding>();
                if (soldier != null)
                {
                    soldier.Serve(building);
                }
            }
            if (target.tag == "Enemy")
            {
                Health targetHealth = target.GetComponent<Health>();
                if (soldier != null && targetHealth != null)
                {
                    soldier.AttackTarget(targetHealth);
                }
            }
        }

        private bool AutoGathererBehavior()
        {
            if (
                unit.assignedBuilding is ProductionBuilding productionBuilding
                && timeSinceCurrentActionIsNull > 4f
            )
            {
                gatherer.Gather(productionBuilding);
                return true;
            }
            return false;
        }

        private bool AutoCombatBehavior()
        {
            if (unit.assignedBuilding is MilitaryBuilding)
            {
                soldier.Patrol();
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
