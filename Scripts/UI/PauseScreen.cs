using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseScreen : MonoBehaviour
{
    [SerializeField]
    string mainMenuSceneName = "MainMenuScene";

    [SerializeField]
    GameObject pauseScreen;

    [SerializeField]
    InputActionAsset UIActionAsset;

    private InputAction _pauseAction;

    private bool isPaused = false;

    void OnEnable()
    {
        var map = UIActionAsset.FindActionMap("UI");
        _pauseAction = map.FindAction("Esc");
        _pauseAction.performed += ctx => PauseUnPauseGame();
        _pauseAction.Enable();
    }

    void OnDisable()
    {
        _pauseAction.performed -= ctx => PauseUnPauseGame();
        _pauseAction.Disable();
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void PauseUnPauseGame()
    {
        if (!isPaused)
        {
            pauseScreen.SetActive(true);
            isPaused = true;
            Time.timeScale = 0f;
        }
        else
        {
            pauseScreen.SetActive(false);
            isPaused = false;
            Time.timeScale = 1f;
        }
    }
}
