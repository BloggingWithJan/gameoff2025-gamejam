using UnityEngine;

public class FadeOnAwake : MonoBehaviour
{
    public float fadeDuration = 20.0f;

    private CanvasGroup group;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();

        group.alpha = 0f;

        // Fade out automatically
        StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha +=  (t / fadeDuration);
            yield return null;
        }

        group.alpha = 1f;
    }
}