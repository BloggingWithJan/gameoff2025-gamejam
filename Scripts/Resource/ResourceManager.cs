using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Resource
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private TMP_Text woodText;
        [SerializeField] private TMP_Text stoneText;
        [SerializeField] private TMP_Text coinText;

        [Header("Values")]
        [SerializeField] private int wood;
        [SerializeField] private int stone;
        [SerializeField] private int coin;

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

            // Initialize dictionaries for cleaner access
            _resources = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, wood },
                { ResourceType.Stone, stone },
                { ResourceType.Coins, coin }
            };

            _uiTexts = new Dictionary<ResourceType, TMP_Text>
            {
                { ResourceType.Wood, woodText },
                { ResourceType.Stone, stoneText },
                { ResourceType.Coins, coinText }
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
            if (!_resources.ContainsKey(type)) return;
            _resources[type] += amount;
            UpdateUIText(type);
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
                if (!_resources.ContainsKey(cost.resource))
                {
                    Debug.LogError($"Resource type {cost.resource} not implemented");
                    continue;
                }

                _resources[cost.resource] -= cost.amount;
                UpdateUIText(cost.resource);
            }
        }

        [ContextMenu("Update Resource UI")]
        private void UpdateUI()
        {
            foreach (var type in _resources.Keys)
                UpdateUIText(type);
        }

        private void UpdateUIText(ResourceType type)
        {
            if (_uiTexts.TryGetValue(type, out TMP_Text text))
                text.text = _resources[type].ToString();
        }
    }
}
