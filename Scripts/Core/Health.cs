using System;
using UnityEngine;

namespace GameJam.Core
{
    public class Health : MonoBehaviour
    {
        public float maxHealth;
        public float currentHealth { get; private set; }

        public event Action<float> OnTakeDamage;
        public event Action<float> OnHeal;
        public event Action OnDeath;

        bool isDead = false;

        public bool IsDead()
        {
            return isDead;
        }

        private Animator animator = null;
        private ActionScheduler actionScheduler = null;

        void Awake()
        {
            currentHealth = maxHealth;
        }

        void Start()
        {
            animator = GetComponent<Animator>();
            actionScheduler = GetComponent<ActionScheduler>();
        }

        public void TakeDamage(float amount)
        {
            currentHealth = Mathf.Max(currentHealth - amount, 0);
            // Debug.Log(
            //     $"{gameObject.name} took {amount} damage. Current health: {currentHealth}/{maxHealth}"
            // );
            OnTakeDamage?.Invoke(amount);
            if (currentHealth == 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHeal?.Invoke(amount);
            // Debug.Log(
            //     $"{gameObject.name} healed {amount} health. Current health: {currentHealth}/{maxHealth}"
            // );
        }

        private void Die()
        {
            if (isDead)
                return;

            // Debug.Log($"{gameObject.name} has died.");
            isDead = true;
            OnDeath?.Invoke();

            if (animator != null)
            {
                animator.SetTrigger("die");
            }
            if (actionScheduler != null)
            {
                actionScheduler.CancelCurrentAction();
            }
        }
    }
}
