using GameJam.Core;
using UnityEngine;

namespace GameJam.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/New Weapon", order = 0)]
    public class Weapon : ScriptableObject
    {
        [SerializeField]
        AnimatorOverrideController animatorOverride = null;

        [SerializeField]
        GameObject equippedPrefab = null;

        [SerializeField]
        float weaponDamage = 5f;

        [SerializeField]
        float weaponRange = 2f;

        [SerializeField]
        float timeBetweenAttacks = 1f;

        [SerializeField]
        bool isRightHanded = true;

        [SerializeField]
        Projectile projectile = null;

        [SerializeField]
        AudioClip attackSound = null;

        GameObject instantiatedWeapon = null;

        private Animator animatorController = null;
        private RuntimeAnimatorController originalController = null;

        public void Spawn(
            Transform rigthHandTransform,
            Transform leftHandTransform,
            Animator animator
        )
        {
            if (equippedPrefab != null)
            {
                Transform handTransform = isRightHanded ? rigthHandTransform : leftHandTransform;
                instantiatedWeapon = Instantiate(equippedPrefab, handTransform);
            }

            if (animatorOverride != null)
            {
                animatorController = animator;
                originalController = animator.runtimeAnimatorController;
                animator.runtimeAnimatorController = animatorOverride;
            }
        }

        private Transform GetTransform(Transform rightHand, Transform leftHand)
        {
            return isRightHanded ? rightHand : leftHand;
        }

        public bool HasProjectile()
        {
            return projectile != null;
        }

        public float GetDamage()
        {
            return weaponDamage;
        }

        public float GetRange()
        {
            return weaponRange;
        }

        public float GetTimeBetweenAttacks()
        {
            return timeBetweenAttacks;
        }

        public void LaunchProjectile(Transform rightHand, Transform leftHand, Health target)
        {
            Projectile projectileInstance = Instantiate(
                projectile,
                GetTransform(rightHand, leftHand).position,
                Quaternion.identity
            );
            projectileInstance.SetTarget(target, weaponDamage);
        }

        public void Unequip()
        {
            if (instantiatedWeapon != null)
            {
                Destroy(instantiatedWeapon);
            }

            if (animatorController != null && originalController != null)
            {
                animatorController.runtimeAnimatorController = originalController;
            }
        }

        public AudioClip GetAttackSound()
        {
            return attackSound;
        }
    }
}
