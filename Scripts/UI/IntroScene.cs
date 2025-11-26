using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScene : MonoBehaviour
{
    [SerializeField]
    string mainGameSceneName = "GameJamScene";

    public void StartGame()
    {
        // Load the main game scene
        SceneManager.LoadScene(mainGameSceneName);
    }
}
