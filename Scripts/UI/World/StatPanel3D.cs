using GameJam.Core;
using UnityEngine;

namespace RPG.UI
{
    public class StatPanel3D : MonoBehaviour
    {
        public ResourceBarTracker healthBar;
        private Health healthComponent;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            healthComponent = GetComponentInParent<Health>();

            if (healthComponent)
            {
                healthBar.Setup(
                    (int)healthComponent.currentHealth,
                    (int)healthComponent.maxHealth,
                    true,
                    ResourceBarTracker.ShapeType.RectangleHorizontal,
                    ResourceBarTracker.DisplayType.None,
                    null
                );
                healthComponent.OnTakeDamage += (amount) =>
                {
                    healthBar.gameObject.SetActive(true);
                };
                healthComponent.OnDeath += () =>
                {
                    healthBar.gameObject.SetActive(false);
                };
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (healthComponent.IsDead())
            {
                healthBar.gameObject.SetActive(false);
                return;
            }

            if (healthComponent)
                healthBar.UpdateValues(
                    (int)healthComponent.currentHealth,
                    (int)healthComponent.maxHealth
                );
        }

        // public void SetBarsActive(bool isActive)
        // {
        //     healthBar.gameObject.SetActive(isActive);
        // }
    }
}
