using GameJam.Core;
using GameJam.Movement;
using UnityEngine;

namespace GameJam.Combat
{
    public class Fighter : MonoBehaviour, IAction
    {
        [SerializeField]
        Weapon weapon = null;

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
            if (target == null)
            {
                // Debug.Log("no target");
                actionScheduler.CancelIfCurrentActionIs(this);
                return;
            }
            if (target.IsDead())
            {
                actionScheduler.CancelIfCurrentActionIs(this);
                return;
            }
            if (fighter.IsDead())
                return;

            if (!GetIsInRange())
            {
                GetComponent<Mover>().MoveTo(target.transform.position);
            }
            else
            {
                GetComponent<Mover>().Cancel();
                AttackBehaviour();
            }
        }

        private bool GetIsInRange()
        {
            return Vector3.Distance(transform.position, target.transform.position)
                < weapon.GetRange();
        }

        private void AttackBehaviour()
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
    }
}
