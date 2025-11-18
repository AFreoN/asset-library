using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CPAM
{
    /// <summary>
    /// Adds context menu items to the Project window for adding assets to the library.
    /// </summary>
    public static class ContextMenuExtension
    {
        /// <summary>
        /// Context menu item: Right-click asset → "Add to Asset Library"
        /// </summary>
        [MenuItem("Assets/Add to Asset Library", priority = 20)]
        public static void AddToAssetLibrary()
        {
            var selectedAssets = GetSelectedAssets();

            if (selectedAssets.Count == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select at least one asset to add to the library.", "OK");
                return;
            }

            // Open the Add Asset Dialog
            AddAssetDialog.ShowDialog(selectedAssets);
        }

        /// <summary>
        /// Validation function for the context menu item.
        /// Menu is only shown when assets are selected.
        /// </summary>
        [MenuItem("Assets/Add to Asset Library", validate = true)]
        public static bool ValidateAddToAssetLibrary()
        {
            return GetSelectedAssets().Count > 0;
        }

        /// <summary>
        /// Get all selected assets in the Project window.
        /// </summary>
        private static List<SelectedAsset> GetSelectedAssets()
        {
            var selectedAssets = new List<SelectedAsset>();
            var selectedGuids = Selection.assetGUIDs;

            foreach (var guid in selectedGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                // Skip folders
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    continue;
                }

                var assetType = GetAssetTypeFromPath(assetPath);
                var assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

                selectedAssets.Add(new SelectedAsset
                {
                    Path = assetPath,
                    Name = assetName,
                    Type = assetType,
                    FullPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), assetPath)
                });
            }

            return selectedAssets;
        }

        /// <summary>
        /// Determine the asset type from the file path.
        /// </summary>
        public static string GetAssetTypeFromPath(string assetPath)
        {
            var extension = System.IO.Path.GetExtension(assetPath).ToLower();

            // Handle specific file types
            return extension switch
            {
                ".prefab" => "GameObject",
                ".png" => "Texture2D",
                ".jpg" => "Texture2D",
                ".jpeg" => "Texture2D",
                ".tga" => "Texture2D",
                ".psd" => "Texture2D",
                ".exr" => "Texture2D",
                ".cs" => "MonoScript",
                ".mat" => "Material",
                ".shader" => "Shader",
                ".compute" => "ComputeShader",
                ".mp3" => "AudioClip",
                ".wav" => "AudioClip",
                ".ogg" => "AudioClip",
                ".aif" => "AudioClip",
                ".aiff" => "AudioClip",
                ".fbx" => "Mesh",
                ".obj" => "Mesh",
                ".blend" => "Mesh",
                ".anim" => "AnimationClip",
                ".controller" => "AnimatorController",
                _ => "Object"
            };
        }

        /// <summary>
        /// Data class for selected asset information.
        /// </summary>
        public class SelectedAsset
        {
            public string Path { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string FullPath { get; set; }
        }
    }
}
