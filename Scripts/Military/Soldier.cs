using GameJam.Core;
using GameJam.Movement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR.Haptics;

namespace GameJam.Military
{
    public class Soldier : MonoBehaviour, IAction
    {
        public enum SoldierState
        {
            Idle,
            MoveToMilitaryBuilding,
            AutomaticCombat,
        }

        public SoldierState currentState;

        private MilitaryBuilding militaryBuilding;
        private SoldierType currentSoldierType;

        private Health health;
        private Mover mover;
        private Unit unit;
        private NavMeshAgent navMeshAgent;

        void Start()
        {
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            unit = GetComponent<Unit>();
            navMeshAgent = GetComponent<NavMeshAgent>();
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
                case SoldierState.AutomaticCombat:
                    AutomaticCombat();
                    break;
            }
        }

        public void Serve(MilitaryBuilding building)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            if (militaryBuilding == building)
            {
                currentState = SoldierState.MoveToMilitaryBuilding;
                return;
            }

            if (building.RequestSoldierSlot(this))
            {
                if (militaryBuilding != null)
                {
                    militaryBuilding.ReleaseSoldierSlot(this);
                }
                militaryBuilding = building;
                currentState = SoldierState.MoveToMilitaryBuilding;
            }
        }

        private void AutomaticCombat() { }

        private void MoveToMilitaryBuilding()
        {
            mover.MoveTo(militaryBuilding.transform.position);
            if (mover.IsDestinationReached())
            {
                Debug.Log("Reached military building " + militaryBuilding.name);
                SetSoldierType(militaryBuilding.GetSoldierType());
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
            navMeshAgent.isStopped = true;
            currentState = SoldierState.Idle;
        }
    }
}
