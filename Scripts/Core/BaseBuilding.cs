using System;
using System.Collections.Generic;
using GameJam.Core;
using GameJam.Military;
using GameJam.Production;
using UnityEngine;

namespace Core
{
    public abstract class BaseBuilding : MonoBehaviour
    {
        public event Action OnBuildingDestroyed;

        [SerializeField]
        public string buildingName;

        [SerializeField]
        public string description;

        [SerializeField]
        public int maxUnitSlots = 3;

        [SerializeField]
        ParticleSystem damagedEffect;

        [SerializeField]
        AudioClip destroySound;

        [SerializeField]
        Transform spawnPoint;

        [SerializeField]
        public bool isMovable = true;

        [SerializeField]
        public bool isDismantable = true;

        [Header("Costs")]
        public List<ResourceCost> costs;

        [Header("Refund")]
        public List<ResourceCost> refunds;
        
        [HideInInspector] public bool IsPreview = false;

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
            Debug.Log($"{buildingName} took {damageAmount} damage.");
            if (damagedEffect != null)
            {
                damagedEffect.Play();
            }
        }

        private void HandleDeath()
        {
            Debug.Log($"{buildingName} destroyed.");
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

        public bool HasAvailableUnitSlots()
        {
            return assignedUnits.Count < maxUnitSlots;
        }

        public string GetUnitSlotsInfos()
        {
            return $"Assigned Units: {assignedUnits.Count}/{maxUnitSlots}";
        }

        public Vector3 GetSpawnPoint()
        {
            return spawnPoint.position;
        }

        public abstract SoldierType GetSoldierType();
        public abstract GathererType GetGathererType();
    }
}
