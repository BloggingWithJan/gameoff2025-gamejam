using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField]
    string gameWonScene = "GameWonScene";

    [SerializeField]
    string gameLostScene = "GameLostScene";

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
        GameObject baseObj = GameObject.FindGameObjectWithTag("PlayerBase");
        if (baseObj != null)
        {
            baseObj.GetComponent<BaseBuilding>().OnBuildingDestroyed += LoadGameLostScene;
        }
        else
        {
            Debug.LogError("Player base not found in the scene!");
        }
        WaveManager.onAllWavesCompleted += LoadGameWonScene;
    }

    public void LoadGameWonScene()
    {
        SceneManager.LoadScene(gameWonScene);
    }

    public void LoadGameLostScene()
    {
        SceneManager.LoadScene(gameLostScene);
    }
}
