using System;
using System.Collections;
using System.Collections.Generic;
using Controller;
using GameJam.Combat;
using GameJam.Core;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    public static event Action onAllWavesCompleted;

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

    [Header("UI References")] [SerializeField]
    private TMP_Text currentWaveText;

    [SerializeField] private TMP_Text nextWaveText;

    [Header("Wave Settings")] [SerializeField]
    private List<Wave> waves = new List<Wave>();

    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private Transform playerBase;

    [SerializeField] float initialDelay = 60f;

    [SerializeField] float timeBetweenWaves = 120f;

    [Header("Cinematics")] [SerializeField]
    CinemachineCamera waveIntroCamera;

    [SerializeField] float introCameraDuration = 3f;

    [SerializeField] List<GameObject> uiControlsToHideDuringIntro;

    [SerializeField] List<GameObject> uiControlsToShowDuringIntro;

    [Header("Ghost Ship Settings")] [SerializeField]
    private GameObject ghostShipPrefab;

    [SerializeField] private Transform islandCenter;
    [SerializeField] private float waterLevel = 0f;
    [SerializeField] private float shipEntryDistance = 50f;

    private int currentWave = 0;
    private float countdownToNextWave;
    private Coroutine waveCoroutine;
    private List<Health> lastWaveEnemies = new List<Health>();
    private int lastWaveEnemiesAlive = 0;
    private Dictionary<GameObject, bool> uiStateBeforeIntro = new Dictionary<GameObject, bool>();

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
                    bool isLastWave = (waveIndex == waves.Count - 1);
                    yield return StartCoroutine(SpawnEnemyWave(waves[waveIndex], isLastWave));

                    // If this was the last wave, stop spawning
                    if (isLastWave)
                    {
                        Debug.Log("Last wave spawned! Waiting for enemies to be defeated...");
                        yield break;
                    }
                }
                else
                {
                    Debug.LogWarning($"No wave configured for wave {currentWave}");
                    yield break;
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

    IEnumerator SpawnEnemyWave(Wave wave, bool isLastWave)
    {
        Debug.Log($"Spawning wave: {wave.waveName}");
        yield return StartCoroutine(PlayCinemachineIntro());

        if (isLastWave)
        {
            lastWaveEnemies.Clear();
            lastWaveEnemiesAlive = 0;
        }

        // pick a random spawn point for the ship to crash on
        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        Vector3 targetPos = spawnPoint.position;

// calculate where the ship enters from
        Vector3 entryPos = ComputeShipEntryPoint(targetPos);

// spawn the ghost ship
        GameObject ship = Instantiate(ghostShipPrefab);
        var controller = ship.GetComponent<GhostShipController>();

        bool waveSpawned = false;

        controller.StartSequence(entryPos, targetPos, waterLevel);
        controller.onCrash = () =>
        {
            if (!waveSpawned)
            {
                waveSpawned = true;
                StartCoroutine(SpawnEnemiesAfterShipCrash(wave, isLastWave, spawnPoint));
            }
        };

// wait until ship triggers its crash callback
        while (!waveSpawned)
        {
            yield return null;
        }
    }

    private IEnumerator SpawnEnemiesAfterShipCrash(Wave wave, bool isLastWave, Transform spawnPoint)
    {
        if (isLastWave)
        {
            lastWaveEnemies.Clear();
            lastWaveEnemiesAlive = 0;
        }

        foreach (EnemySpawnInfo enemyInfo in wave.enemies)
        {
            for (int i = 0; i < enemyInfo.count; i++)
            {
                GameObject enemy = Instantiate(enemyInfo.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

                var aiController = enemy.GetComponent<EnemySoldier>();
                if (aiController != null && playerBase != null)
                    aiController.SetPlayerBase(playerBase);

                if (isLastWave)
                {
                    var health = enemy.GetComponent<Health>();
                    if (health != null)
                    {
                        lastWaveEnemies.Add(health);
                        lastWaveEnemiesAlive++;
                        health.OnDeath += () => OnLastWaveEnemyDied(health);
                    }
                }

                yield return new WaitForSeconds(enemyInfo.spawnInterval);
            }
        }
    }


    GameObject SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return null;
        }

        // Choose random spawn point
        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // Set player base target (if enemy has AI controller)
        var aiController = enemy.GetComponent<EnemySoldier>();
        if (aiController != null && playerBase != null)
        {
            aiController.SetPlayerBase(playerBase);
        }

        Debug.Log($"Spawned {enemyPrefab.name} at {spawnPoint.position}");
        return enemy;
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

    private void OnLastWaveEnemyDied(Health enemyHealth)
    {
        lastWaveEnemiesAlive--;
        Debug.Log($"Last wave enemy died. Remaining: {lastWaveEnemiesAlive}");

        if (lastWaveEnemiesAlive <= 0)
        {
            Debug.Log("All waves completed! Player wins!");
            onAllWavesCompleted?.Invoke();
        }
    }

    private IEnumerator PlayCinemachineIntro()
    {
        //das ganze soll für x sekunden laufen
        //in der zeit wollen wir die UI eigentlich komplett ausblenden
        //deaktivierung von input nicht so wichtig
        if (waveIntroCamera != null)
        {
            SaveAndHideUI();
            waveIntroCamera.enabled = true;
            waveIntroCamera.Priority = 999;

            yield return new WaitForSeconds(introCameraDuration);

            RestoreUI();
            waveIntroCamera.Priority = 0;
            waveIntroCamera.enabled = false;
        }
    }

    private void SaveAndHideUI()
    {
        uiStateBeforeIntro.Clear();

        // Save state and hide UI elements that should be hidden
        foreach (var uiElement in uiControlsToHideDuringIntro)
        {
            if (uiElement != null)
            {
                uiStateBeforeIntro[uiElement] = uiElement.activeSelf;
                uiElement.SetActive(false);
            }
        }

        // Show UI elements that should appear during intro
        foreach (var uiElement in uiControlsToShowDuringIntro)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(true);
            }
        }
    }

    private void RestoreUI()
    {
        // Restore original state of hidden UI elements
        foreach (var kvp in uiStateBeforeIntro)
        {
            if (kvp.Key != null)
            {
                kvp.Key.SetActive(kvp.Value);
            }
        }

        // Hide UI elements that were shown during intro
        foreach (var uiElement in uiControlsToShowDuringIntro)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(false);
            }
        }
    }

    private Vector3 ComputeShipEntryPoint(Vector3 spawnPoint)
    {
        Vector3 outwardDir = (spawnPoint - islandCenter.position).normalized;
        Vector3 entry = spawnPoint + outwardDir * shipEntryDistance;
        entry.y = waterLevel - 8f;
        return entry;
    }
}