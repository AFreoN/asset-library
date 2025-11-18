using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CPAM
{
    /// <summary>
    /// Handles importing assets from the library into the current project.
    /// </summary>
    public static class AssetImporter
    {
        private const string DefaultImportFolder = "Assets/Imported";

        /// <summary>
        /// Import selected assets from the library.
        /// </summary>
        public static void ImportAssets(AssetLibraryLoader loader, List<AssetMetadata> assetsToImport)
        {
            if (loader == null || !loader.IsLoaded)
            {
                EditorUtility.DisplayDialog("Error", "No library loaded.", "OK");
                return;
            }

            if (assetsToImport == null || assetsToImport.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No assets selected for import.", "OK");
                return;
            }

            // Get import destination
            var importPath = GetImportDestination();
            if (string.IsNullOrEmpty(importPath))
            {
                return; // User cancelled
            }

            // Ensure directory exists
            if (!Directory.Exists(importPath))
            {
                Directory.CreateDirectory(importPath);
            }

            EditorUtility.DisplayProgressBar("Importing Assets", "Importing assets...", 0);

            try
            {
                int successCount = 0;
                for (int i = 0; i < assetsToImport.Count; i++)
                {
                    var asset = assetsToImport[i];
                    EditorUtility.DisplayProgressBar("Importing Assets", $"Importing {asset.name}...", (float)i / assetsToImport.Count);

                    if (ImportAsset(loader, asset, importPath))
                    {
                        successCount++;
                    }
                }

                EditorUtility.ClearProgressBar();

                // Refresh asset database
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Import Complete", $"Successfully imported {successCount} of {assetsToImport.Count} asset(s).", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", $"Import failed: {ex.Message}", "OK");
                LibraryUtilities.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Import a single asset.
        /// </summary>
        private static bool ImportAsset(AssetLibraryLoader loader, AssetMetadata asset, string importPath)
        {
            try
            {
                // Get asset file
                var assetData = loader.GetAssetFile(asset);
                if (assetData == null || assetData.Length == 0)
                {
                    LibraryUtilities.LogWarning($"Asset file not found: {asset.name}");
                    return false;
                }

                // Determine destination filename
                var fileName = Path.GetFileName(asset.relativePath);
                var destinationPath = Path.Combine(importPath, fileName);

                // Handle duplicate filenames
                var uniquePath = GetUniqueFilePath(destinationPath);

                // Write asset file
                File.WriteAllBytes(uniquePath, assetData);

                LibraryUtilities.Log($"Imported asset: {asset.name} -> {uniquePath}");
                return true;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogWarning($"Failed to import asset {asset.name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the import destination folder.
        /// </summary>
        private static string GetImportDestination()
        {
            // Try to get saved preference
            var savedPath = EditorPrefs.GetString("CPAM.ImportPath", DefaultImportFolder);

            // Show folder selection dialog
            var path = EditorUtility.SaveFolderPanel(
                "Select Import Destination",
                Path.GetDirectoryName(Application.dataPath) + "/" + (string.IsNullOrEmpty(savedPath) ? "Assets" : savedPath),
                ""
            );

            if (string.IsNullOrEmpty(path))
            {
                return null; // User cancelled
            }

            // Convert to relative path within Assets
            var dataPath = Application.dataPath;
            if (path.StartsWith(dataPath))
            {
                path = "Assets" + path.Substring(dataPath.Length);
            }

            // Save preference
            EditorPrefs.SetString("CPAM.ImportPath", path);

            return path;
        }

        /// <summary>
        /// Get a unique file path if the file already exists.
        /// </summary>
        private static string GetUniqueFilePath(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return filePath;
            }

            var directory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);

            int counter = 1;
            string newPath;

            do
            {
                newPath = Path.Combine(directory, $"{fileName} ({counter}){extension}");
                counter++;
            }
            while (File.Exists(newPath) && counter < 1000); // Safety limit

            return newPath;
        }
    }
}
