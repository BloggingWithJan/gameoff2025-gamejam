using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    string introSceneName = "IntroScene";

    public void StartGame()
    {
        SceneManager.LoadScene(introSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
