using UnityEngine;

public class RandomModelSelector : MonoBehaviour
{
    public GameObject[] models;

    /// <summary>
    /// Randomly activates one model, disables the rest, and applies random Y rotation
    /// </summary>
    public void ApplyRandomModel()
    {
        if (models == null || models.Length == 0)
            return;

        foreach (var m in models)
            if (m != null)
                m.SetActive(false);

        // Unique seed per object to avoid duplicates
        System.Random rnd = new System.Random(GetInstanceID() ^ System.Environment.TickCount);
        int i = rnd.Next(models.Length);

        if (models[i] != null)
        {
            models[i].SetActive(true);

            // Apply random Y rotation
            float randomY = (float)rnd.NextDouble() * 360f;
            models[i].transform.localRotation = Quaternion.Euler(0f, randomY, 0f);
        }
    }
}