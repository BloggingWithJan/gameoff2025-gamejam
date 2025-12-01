using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public enum FadeType { FadeIn, FadeOut }

    [Header("Fade Settings")]
    public FadeType fadeType = FadeType.FadeOut;
    public float fadeDuration = 2.0f;
    public float fadeDelay = 0f; // Delay before fade starts

    private CanvasGroup group;

    private void Awake()
    {
        Debug.Log("Awake");
        group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();

        // Set initial alpha based on fade type
        group.alpha = fadeType == FadeType.FadeIn ? 0f : 1f;

        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        // Wait for the delay before starting fade
        if (fadeDelay > 0f)
            yield return new WaitForSeconds(fadeDelay);

        float elapsed = 0f;
        float startAlpha = group.alpha;
        float endAlpha = fadeType == FadeType.FadeIn ? 1f : 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            yield return null;
        }

        group.alpha = endAlpha; // ensure exact value at end
    }
}