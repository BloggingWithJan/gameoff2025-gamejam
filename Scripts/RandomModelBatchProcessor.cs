using UnityEngine;
using UnityEditor;
using System;

public static class RandomModelBatchProcessor
{
    [MenuItem("Tools/Randomize All RandomModelSelectors in Scene")]
    private static void RandomizeAllInScene()
    {
        var selectors = GameObject.FindObjectsOfType<RandomModelSelector>();
        foreach (var selector in selectors)
        {
            selector.ApplyRandomModel();
            EditorUtility.SetDirty(selector.gameObject);
        }

        Debug.Log($"Randomized {selectors.Length} RandomModelSelectors in the scene.");
    }
}