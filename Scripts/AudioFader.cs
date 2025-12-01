using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioFader : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeDuration = 2f;

    private void Awake()
    {
        Debug.Log("Awake?");
        if (audioSource == null) {
            audioSource = GetComponent<AudioSource>();
            Debug.Log("Awake123?");
        }
    }

    /// <summary>
    /// Fade the audio out to zero volume over fadeDuration.
    /// </summary>
    public void FadeOut()
    {
        Debug.Log("Fade Out");
        StartCoroutine(FadeTo(0f));
    }

    /// <summary>
    /// Fade the audio in to its original volume over fadeDuration.
    /// </summary>
    public void FadeIn(float targetVolume = 1f)
    {
        StartCoroutine(FadeTo(targetVolume));
    }

    private IEnumerator FadeTo(float targetVolume)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}