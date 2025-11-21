using System.Collections.Generic;
using Data;
using TMPro;
using UnityEngine;

namespace GameJam.Resource
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField]
        private TMP_Text woodText;

        [SerializeField]
        private TMP_Text stoneText;

        [SerializeField]
        private TMP_Text foodText;

        [SerializeField]
        private TMP_Text populationText;

        [Header("Values")]
        [SerializeField]
        private int wood;

        [SerializeField]
        private int stone;

        [SerializeField]
        private int food;

        [SerializeField]
        private int currentPopulation;

        [SerializeField]
        private int maxPopulation;

        public int CurrentPopulation => currentPopulation;
        public int MaxPopulation => maxPopulation;

        private Dictionary<ResourceType, int> _resources;
        private Dictionary<ResourceType, TMP_Text> _uiTexts;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _resources = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, wood },
                { ResourceType.Stone, stone },
                { ResourceType.Food, food },
            };

            _uiTexts = new Dictionary<ResourceType, TMP_Text>
            {
                { ResourceType.Wood, woodText },
                { ResourceType.Stone, stoneText },
                { ResourceType.Food, foodText },
            };
        }

        private void Start()
        {
            UpdateUI();
        }

        public int GetResource(ResourceType type) => _resources[type];

        public void SetResource(ResourceType type, int amount)
        {
            _resources[type] = Mathf.Max(0, amount);
            UpdateUIText(type);
        }

        public void AddResource(ResourceType type, int amount)
        {
            if (!_resources.ContainsKey(type))
                return;

            _resources[type] += amount;
            UpdateUIText(type);
        }

        public void SetPopulation(int value)
        {
            currentPopulation = Mathf.Clamp(value, 0, maxPopulation);
            UpdatePopulationUI();
        }

        public void AddPopulation(int value)
        {
            SetPopulation(currentPopulation + value);
        }

        public void SetMaxPopulation(int value)
        {
            maxPopulation = Mathf.Max(0, value);

            if (currentPopulation > maxPopulation)
                currentPopulation = maxPopulation;

            UpdatePopulationUI();
        }

        public void AddMaxPopulation(int value)
        {
            SetMaxPopulation(maxPopulation + value);
            UpdatePopulationUI();
        }

        public void DeductMaxPopulation(int value)
        {
            SetMaxPopulation(maxPopulation - value);
            UpdatePopulationUI();
        }

        public void DeductPopulation(int value)
        {
            SetPopulation(currentPopulation - value);
            UpdatePopulationUI();
        }

        public bool MaxPopulationReached()
        {
            return currentPopulation >= maxPopulation;
        }

        public bool HasSufficientResources(ResourceCost cost)
        {
            if (!_resources.ContainsKey(cost.resource))
            {
                Debug.LogError($"Resource type {cost.resource} not implemented");
                return false;
            }

            if (_resources[cost.resource] < cost.amount)
                return false;

            return true;
        }

        public bool HasSufficientResources(BuildingData buildingData)
        {
            foreach (var cost in buildingData.costs)
            {
                if (!_resources.ContainsKey(cost.resource))
                {
                    Debug.LogError($"Resource type {cost.resource} not implemented");
                    return false;
                }

                if (_resources[cost.resource] < cost.amount)
                    return false;
            }

            return true;
        }

        public void DeductResources(BuildingData buildingData)
        {
            foreach (var cost in buildingData.costs)
            {
                if (cost.resource == ResourceType.Population)
                {
                    SetPopulation(currentPopulation - cost.amount);
                    continue;
                }

                if (!_resources.ContainsKey(cost.resource))
                {
                    Debug.LogError($"Resource type {cost.resource} not implemented");
                    continue;
                }

                _resources[cost.resource] -= cost.amount;
                UpdateUIText(cost.resource);
            }
        }

        public List<ResourceCost> RefundResourcesPartially(
            BuildingData buildingData,
            float refundPercentage = 0.5f
        )
        {
            var refundedResources = new List<ResourceCost>();

            if (buildingData == null || buildingData.costs == null)
                return refundedResources;

            foreach (var cost in buildingData.costs)
            {
                int refundAmount = Mathf.FloorToInt(cost.amount * refundPercentage);
                if (refundAmount <= 0)
                    continue;

                if (cost.resource == ResourceType.Population)
                {
                    AddPopulation(refundAmount);
                    refundedResources.Add(new ResourceCost(ResourceType.Population, refundAmount));
                    continue;
                }

                if (!_resources.ContainsKey(cost.resource))
                {
                    Debug.LogError($"Resource type {cost.resource} not implemented for refund.");
                    continue;
                }

                _resources[cost.resource] += refundAmount;
                UpdateUIText(cost.resource);
                refundedResources.Add(new ResourceCost(cost.resource, refundAmount));
            }

            return refundedResources;
        }

        [ContextMenu("Update Resource UI")]
        private void UpdateUI()
        {
            foreach (var type in _resources.Keys)
                UpdateUIText(type);

            UpdatePopulationUI();
        }

        private void UpdateUIText(ResourceType type)
        {
            if (_uiTexts.TryGetValue(type, out TMP_Text text))
                text.text = _resources[type].ToString();

            wood = _resources[ResourceType.Wood];
            stone = _resources[ResourceType.Stone];
            food = _resources[ResourceType.Food];
        }

        private void UpdatePopulationUI()
        {
            populationText.text = $"{currentPopulation} / {maxPopulation}";
        }

        [ContextMenu("Sync From Inspector")]
        private void SyncFromInspector()
        {
            _resources[ResourceType.Wood] = wood;
            _resources[ResourceType.Stone] = stone;
            _resources[ResourceType.Food] = food;

            currentPopulation = Mathf.Clamp(currentPopulation, 0, maxPopulation);
            UpdateUI();
        }
    }
}
