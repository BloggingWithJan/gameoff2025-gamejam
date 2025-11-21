using System.Collections;
using System.Collections.Generic;
using GameJam.Combat;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [System.Serializable]
    public class EnemySpawnInfo
    {
        public GameObject enemyPrefab;
        public int count;

        [Tooltip("Delay between spawning each enemy of this type")]
        public float spawnInterval = 0.5f;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";
        public List<EnemySpawnInfo> enemies = new List<EnemySpawnInfo>();
    }

    [Header("UI References")]
    [SerializeField]
    private TMP_Text currentWaveText;

    [SerializeField]
    private TMP_Text nextWaveText;

    [Header("Wave Settings")]
    [SerializeField]
    private List<Wave> waves = new List<Wave>();

    [SerializeField]
    private Transform[] spawnPoints;

    [SerializeField]
    private Transform playerBase;

    [SerializeField]
    float initialDelay = 60f;

    [SerializeField]
    float timeBetweenWaves = 120f;

    private int currentWave = 0;
    private float countdownToNextWave;
    private Coroutine waveCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (waveCoroutine == null)
        {
            waveCoroutine = StartCoroutine(ManageWaves());
        }
    }

    IEnumerator ManageWaves()
    {
        while (true)
        {
            if (currentWave == 0)
            {
                countdownToNextWave = initialDelay;
            }
            else
            {
                countdownToNextWave = timeBetweenWaves;

                // Check if we have a wave configured for this index
                int waveIndex = currentWave - 1; // Wave 1 = index 0
                if (waveIndex < waves.Count)
                {
                    yield return StartCoroutine(SpawnEnemyWave(waves[waveIndex]));
                }
                else
                {
                    Debug.LogWarning($"No wave configured for wave {currentWave}");
                }
            }

            UpdateWaveUI();
            while (countdownToNextWave > 0)
            {
                yield return null;
                countdownToNextWave -= Time.deltaTime;
                UpdateWaveUI();
            }
            currentWave++;
        }
    }

    IEnumerator SpawnEnemyWave(Wave wave)
    {
        Debug.Log($"Spawning wave: {wave.waveName}");

        foreach (EnemySpawnInfo enemyInfo in wave.enemies)
        {
            for (int i = 0; i < enemyInfo.count; i++)
            {
                SpawnEnemy(enemyInfo.enemyPrefab);
                yield return new WaitForSeconds(enemyInfo.spawnInterval);
            }
        }
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        // Choose random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // Set player base target (if enemy has AI controller)
        var aiController = enemy.GetComponent<EnemySoldier>();
        if (aiController != null && playerBase != null)
        {
            aiController.SetPlayerBase(playerBase);
        }

        Debug.Log($"Spawned {enemyPrefab.name} at {spawnPoint.position}");
    }

    void UpdateWaveUI()
    {
        currentWaveText.text = "Wave: " + currentWave;
        nextWaveText.text = "Next Wave In: " + FormatTime(countdownToNextWave);
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Optional: Manually trigger next wave (for testing or gameplay feature)
    public void TriggerNextWave()
    {
        if (countdownToNextWave > 0)
        {
            countdownToNextWave = 0; // Skip countdown
        }
    }
}
