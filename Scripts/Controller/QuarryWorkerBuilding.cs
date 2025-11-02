using UnityEngine;

public class QuarryWorkerBuilding : MonoBehaviour
{
    public GameObject workerPrefab;
    public Transform spawnPoint;
    public float spawnInterval = 10f;
    public float initialDelay = 2f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnWorker), initialDelay, spawnInterval);
    }

    void SpawnWorker()
    {
        Instantiate(workerPrefab, spawnPoint.position, Quaternion.identity);
    }
}
