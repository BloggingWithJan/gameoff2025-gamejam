using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomModelSelector))]
public class RandomModelSelectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RandomModelSelector selector = (RandomModelSelector)target;

        if (GUILayout.Button("Randomize This Model"))
        {
            selector.ApplyRandomModel();
            EditorUtility.SetDirty(selector.gameObject);
        }

        if (GUILayout.Button("Randomize All Selected"))
        {
            RandomizeSelected();
        }
    }

    private void RandomizeSelected()
    {
        foreach (var obj in Selection.gameObjects)
        {
            RandomModelSelector selector = obj.GetComponent<RandomModelSelector>();
            if (selector != null)
            {
                selector.ApplyRandomModel();
                EditorUtility.SetDirty(obj);
            }
        }
    }
}