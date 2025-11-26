using UnityEngine;
using UnityEngine.SceneManagement;

public class GameWonScene : MonoBehaviour
{
    [SerializeField]
    string mainMenuSceneName = "MainMenuScene";

    [SerializeField]
    string gameSceneName = "GameJamScene";

    public void RestartGame()
    {
        // Reload the current scene to restart the game
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitToMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
