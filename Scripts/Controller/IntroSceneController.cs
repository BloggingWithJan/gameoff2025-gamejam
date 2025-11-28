using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class IntroSceneController : MonoBehaviour
{
    [SerializeField]
    private string sceneToLoad = "GameJamScene";

    [SerializeField]
    private float additionalDelay = 0f;

    private PlayableDirector playableDirector;

    void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();
        playableDirector.Play();

        StartCoroutine(WaitForTimelineEnd());
    }

    IEnumerator WaitForTimelineEnd()
    {
        // Wait for the timeline duration
        yield return new WaitForSeconds((float)playableDirector.duration + additionalDelay);

        Debug.Log("Timeline finished, loading scene: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}
