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
        Transform rightHandTransform = null;

        [SerializeField]
        Transform leftHandTransform = null;

        Health target;
        Health fighter;
        float timeSinceLastAttack = Mathf.Infinity;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            SpawnWeapon();
            fighter = GetComponent<Health>();
        }

        // Update is called once per frame
        void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            if (target == null)
                return;
            if (target.IsDead())
                return;
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

        private void SpawnWeapon()
        {
            if (weapon == null)
                return;
            Animator animator = GetComponent<Animator>();
            weapon.Spawn(rightHandTransform, leftHandTransform, animator);
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
