using Core;
using GameJam.Combat;
using GameJam.Core;
using GameJam.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam.Military
{
    public class Soldier : MonoBehaviour, IAction
    {
        public enum SoldierState
        {
            Idle,
            MoveToMilitaryBuilding,
            Patrol,
            AutomaticCombat,
        }

        public SoldierState currentState;

        private SoldierType currentSoldierType;
        private bool isEquipped = false; // Tracks if soldier has reached building and been equipped

        private Health health;
        private Mover mover;
        private Unit unit;
        private Fighter fighter;
        private NavMeshAgent navMeshAgent;
        private ActionScheduler actionScheduler;

        void Start()
        {
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            unit = GetComponent<Unit>();
            fighter = GetComponent<Fighter>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            actionScheduler = GetComponent<ActionScheduler>();
        }

        void Update()
        {
            if (health.IsDead())
                return;
            if (currentState == SoldierState.Idle)
                return;

            switch (currentState)
            {
                case SoldierState.MoveToMilitaryBuilding:
                    MoveToMilitaryBuilding();
                    break;
                case SoldierState.Patrol:
                    AutomaticPatrol();
                    break;
                case SoldierState.AutomaticCombat:
                    AutomaticCombat();
                    break;
            }
        }

        public void Serve(MilitaryBuilding building)
        {
            GetComponent<ActionScheduler>().StartAction(this);

            // Reset equipped state when serving a new building
            isEquipped = false;

            if (unit.assignedBuilding == building)
            {
                currentState = SoldierState.MoveToMilitaryBuilding;
                return;
            }

            if (building.RequestUnitSlot(unit))
            {
                if (unit.assignedBuilding != null)
                {
                    unit.assignedBuilding.ReleaseUnitSlot(unit);
                }
                unit.assignedBuilding = building;
                currentState = SoldierState.MoveToMilitaryBuilding;
            }
        }

        public void Patrol()
        {
            if (!isEquipped)
                return;

            GetComponent<ActionScheduler>().StartAction(this);
            currentState = SoldierState.Patrol;
        }

        public void AttackTarget(Health target)
        {
            if (!isEquipped)
                return;

            GetComponent<ActionScheduler>().StartAction(this);
            fighter.SetCurrentTarget(target);
            currentState = SoldierState.AutomaticCombat;
        }

        private void AutomaticPatrol()
        {
            Health targetInRange = fighter.ScanForTargetInRange();
            if (targetInRange != null)
            {
                fighter.SetCurrentTarget(targetInRange);
                currentState = SoldierState.AutomaticCombat;
            }
        }

        private void AutomaticCombat()
        {
            if (fighter.GetCurrentTarget() == null || fighter.GetCurrentTarget().IsDead())
            {
                navMeshAgent.isStopped = false;
                currentState = SoldierState.Patrol;
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

        private void MoveToMilitaryBuilding()
        {
            mover.MoveTo(unit.assignedBuilding.transform.position);
            if (mover.IsDestinationReached())
            {
                SetSoldierType(unit.assignedBuilding.GetSoldierType());
                isEquipped = true; // Mark as equipped after reaching building
                currentState = SoldierState.Patrol;
                mover.MoveToWithRandomOffset(unit.assignedBuilding.transform.position, 4f);
            }
        }

        private void SetSoldierType(SoldierType newSoldierType)
        {
            if (currentSoldierType == newSoldierType)
            {
                currentState = SoldierState.AutomaticCombat;
                return;
            }

            if (newSoldierType == null)
                return;

            currentSoldierType = newSoldierType;
            unit.ChangeMeshes(newSoldierType.GetHeadMesh(), newSoldierType.GetBodyMesh());
            unit.SpawnWeapon(newSoldierType.GetWeaponPrefab());

            currentState = SoldierState.AutomaticCombat;
        }

        public void Cancel()
        {
            fighter.Cancel();
            navMeshAgent.isStopped = true;
            currentState = SoldierState.Idle;
        }
    }
}
