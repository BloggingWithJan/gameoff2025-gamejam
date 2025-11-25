#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class PrefabThumbnailExtractor
{
    // 1. Add a menu item to the Unity Editor under 'Assets'
    [MenuItem("Assets/Extract Prefab Thumbnail")]
    public static void ExtractThumbnailsFromSelection()
    {
        // Define the folder where extracted images will be saved
        string outputDirectory = "Assets/Extracted_Thumbnails/";
        
        // Ensure the directory exists
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
            Debug.Log($"Created directory: {outputDirectory}");
        }

        // 2. Iterate through all selected assets in the Project window
        foreach (Object selectedObject in Selection.objects)
        {
            // Check if the selected object is a Prefab (GameObject type in the asset database)
            if (selectedObject is GameObject)
            {
                // 3. Use AssetPreview to get the thumbnail Texture2D
                Texture2D thumbnail = AssetPreview.GetAssetPreview(selectedObject);
                
                if (thumbnail != null)
                {
                    // 4. Encode the texture to a PNG byte array
                    // NOTE: This will include the default background color Unity uses for previews.
                    byte[] bytes = thumbnail.EncodeToPNG();
                    
                    // 5. Define the save path
                    string path = outputDirectory + selectedObject.name + "_Preview.png";

                    // The path must be converted from relative to absolute for file system access
                    string fullPath = Application.dataPath + "/" + path.Replace("Assets/", "");
                    
                    // 6. Write the bytes to a file
                    File.WriteAllBytes(fullPath, bytes);
                    Debug.Log($"Successfully extracted and saved thumbnail for '{selectedObject.name}' to: {path}");
                }
                else
                {
                    Debug.LogWarning($"Could not immediately get preview for '{selectedObject.name}'. It might still be generating.");
                    // For immediate generation, you might need to call AssetPreview.GetAssetPreview 
                    // multiple times over a few frames or force generation, but for simple extraction
                    // this often works instantly.
                }
            }
        }

        // 7. Refresh the AssetDatabase to show the new files in the Project window
        AssetDatabase.Refresh();
    }
}
#endif