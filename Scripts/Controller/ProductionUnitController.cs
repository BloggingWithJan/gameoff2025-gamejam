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

        private Health health;
        private Gatherer gatherer;

        void Start()
        {
            health = GetComponent<Health>();
            gatherer = GetComponent<Gatherer>();
            if (homeBuilding != null && gatherer != null)
            {
                gatherer.Gather(homeBuilding);
            }
        }

        void Update()
        {
            if (health.IsDead())
                return;

            // if (currentMode == UnitMode.Automatic)
            //     GathererBehaviour();

            // if (
            //     currentMode == UnitMode.Automatic
            //     && GetComponent<ActionScheduler>().GetCurrentAction() is Fighter
            // )
            // {
            //     //AttackBehaviour();
            //     //return;
            // }
            // if (
            //     currentMode == UnitMode.Automatic
            //     && GetComponent<ActionScheduler>().GetCurrentAction() is Gatherer
            // )
            // {
            //     GathererBehaviour();
            //     return;
            // }

            // if (currentMode == UnitMode.Automatic)
            // {
            //     // AutomaticBehaviour();
            //     GathererBehaviour();
            // }
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
    }
}
