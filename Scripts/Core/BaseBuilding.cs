using System;
using System.Collections.Generic;
using GameJam.Military;
using GameJam.Production;
using UnityEngine;

namespace GameJam.Core
{
    public abstract class BaseBuilding : MonoBehaviour
    {
        public enum BuildingType
        {
            Military,
            Production,
        }

        public event Action OnBuildingDestroyed;

        [SerializeField]
        private string buildingName;

        [SerializeField]
        private string description;

        [SerializeField]
        private BuildingType buildingType;

        [SerializeField]
        private int maxUnitSlots = 3;

        [SerializeField]
        ParticleSystem damagedEffect;

        [SerializeField]
        AudioClip destroySound;

        [SerializeField]
        Transform spawnPoint;

        private AudioSource audioSource;

        private List<Unit> assignedUnits = new List<Unit>();

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.OnTakeDamage += HandleTakeDamage;
                health.OnDeath += HandleDeath;
            }
        }

        private void HandleTakeDamage(float damageAmount)
        {
            if (damagedEffect != null)
            {
                damagedEffect.Play();
            }
        }

        private void HandleDeath()
        {
            OnBuildingDestroyed?.Invoke();
            audioSource.PlayOneShot(destroySound);
            Destroy(gameObject, .5f);
        }

        public bool RequestUnitSlot(Unit unit)
        {
            // Check if already assigned
            if (assignedUnits.Contains(unit))
            {
                Debug.LogWarning($"Unit {unit.name} already assigned to {gameObject.name}");
                return true;
            }

            // Check if slots available
            if (assignedUnits.Count < maxUnitSlots)
            {
                assignedUnits.Add(unit);
                return true;
            }

            return false;
        }

        public void ReleaseUnitSlot(Unit unit)
        {
            if (assignedUnits.Contains(unit))
            {
                assignedUnits.Remove(unit);
            }
            else
            {
                Debug.LogWarning(
                    $"Tried to release unit {unit.name} that wasn't assigned to {gameObject.name}"
                );
            }
        }

        public Vector3 GetSpawnPoint()
        {
            return spawnPoint.position;
        }

        public abstract SoldierType GetSoldierType();
        public abstract GathererType GetGathererType();
    }
}
