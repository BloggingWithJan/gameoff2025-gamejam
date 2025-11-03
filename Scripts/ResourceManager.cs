using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("UI References")] [SerializeField]
    private TMP_Text woodText;

    [SerializeField] private TMP_Text stoneText;
    [SerializeField] private TMP_Text coinText;

    [Header("Values")] [SerializeField] private int wood;
    [SerializeField] private int stone;
    [SerializeField] private int coin;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    public int Wood
    {
        get => wood;
        set
        {
            if (wood == value) return;
            wood = Mathf.Max(0, value);
            UpdateUI();
        }
    }

    public int Stone
    {
        get => stone;
        set
        {
            if (stone == value) return;
            stone = Mathf.Max(0, value);
            UpdateUI();
        }
    }

    public int Coin
    {
        get => coin;
        set
        {
            if (coin == value) return;
            coin = Mathf.Max(0, value);
            UpdateUI();
        }
    }

    [ContextMenu("Update Resource UI")]
    private void UpdateUI()
    {
        if (woodText != null) woodText.text = wood.ToString();
        if (stoneText != null) stoneText.text = stone.ToString();
        if (coinText != null) coinText.text = coin.ToString();
    }
}