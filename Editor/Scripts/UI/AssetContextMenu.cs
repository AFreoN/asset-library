using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CPAL
{
    /// <summary>
    /// Manages the right-click context menu for assets in the Asset Library window.
    /// Handles all context menu options and their callbacks.
    /// </summary>
    public static class AssetContextMenu
    {
        /// <summary>
        /// Show the context menu for an asset.
        /// </summary>
        public static void Show(AssetMetadata asset, AssetLibraryLoader loader, string libraryPath, Rect position)
        {
            var menu = new GenericMenu();

            // Open
            menu.AddItem(new GUIContent("Open"), false, () => OpenAsset(asset, loader));

            // Show Preview
            menu.AddItem(new GUIContent("Show Preview"), false, () => ShowAssetPreview(asset, loader));

            menu.AddSeparator("");

            // Edit Metadata
            menu.AddItem(new GUIContent("Edit Metadata..."), false, () => EditAssetMetadata(asset, loader, libraryPath));

            menu.AddSeparator("");

            // Export to Folder
            menu.AddItem(new GUIContent("Export to Folder..."), false, () => ExportAsset(asset, loader));

            menu.AddSeparator("");

            // Delete from Library
            menu.AddItem(new GUIContent("Delete from Library"), false, () => DeleteAsset(asset, loader, libraryPath));

            menu.AddSeparator("");

            // Properties
            menu.AddItem(new GUIContent("Properties"), false, () => ShowAssetProperties(asset));

            // Copy Asset Path
            menu.AddItem(new GUIContent("Copy Asset Path"), false, () => CopyAssetPath(asset));

            // Reveal Library File
            menu.AddItem(new GUIContent("Reveal Library File"), false, () => RevealLibraryFile(libraryPath));

            menu.ShowAsContext();
        }

        /// <summary>
        /// Open the asset with the system default application.
        /// </summary>
        private static void OpenAsset(AssetMetadata asset, AssetLibraryLoader loader)
        {
            try
            {
                var assetData = loader.GetAssetFile(asset);
                if (assetData == null || assetData.Length == 0)
                {
                    EditorUtility.DisplayDialog("Error", $"Could not extract asset: {asset.name}", "OK");
                    return;
                }

                // Create temp file
                var tempDir = Path.Combine(Path.GetTempPath(), "CPAL_Open", System.Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                var fileName = Path.GetFileName(asset.relativePath);
                var tempPath = Path.Combine(tempDir, fileName);

                File.WriteAllBytes(tempPath, assetData);

                // Open with default application based on platform
                OpenFileWithDefaultApp(tempPath);

                LibraryUtilities.Log($"Opened asset: {asset.name}");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to open asset: {ex.Message}", "OK");
                LibraryUtilities.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Open file with system default application (cross-platform).
        /// </summary>
        private static void OpenFileWithDefaultApp(string filePath)
        {
            try
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    Process.Start(filePath);
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    Process.Start("open", filePath);
                }
                else if (Application.platform == RuntimePlatform.LinuxEditor)
                {
                    Process.Start("xdg-open", filePath);
                }
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to open file: {ex.Message}");
            }
        }

        /// <summary>
        /// Show the asset preview window.
        /// </summary>
        private static void ShowAssetPreview(AssetMetadata asset, AssetLibraryLoader loader)
        {
            AssetPreviewWindow.ShowPreview(asset, loader);
        }

        /// <summary>
        /// Open the edit metadata dialog.
        /// </summary>
        private static void EditAssetMetadata(AssetMetadata asset, AssetLibraryLoader loader, string libraryPath)
        {
            EditAssetMetadataDialog.ShowDialog(asset, loader, libraryPath);
        }

        /// <summary>
        /// Export the asset to a user-selected folder.
        /// </summary>
        private static void ExportAsset(AssetMetadata asset, AssetLibraryLoader loader)
        {
            try
            {
                var assetData = loader.GetAssetFile(asset);
                if (assetData == null || assetData.Length == 0)
                {
                    EditorUtility.DisplayDialog("Error", $"Could not extract asset: {asset.name}", "OK");
                    return;
                }

                // Show save file panel
                var fileName = Path.GetFileName(asset.relativePath);
                var initialFolder = Application.persistentDataPath;

                var savePath = EditorUtility.SaveFilePanel(
                    "Export Asset",
                    initialFolder,
                    fileName,
                    Path.GetExtension(fileName).TrimStart('.')
                );

                if (string.IsNullOrEmpty(savePath))
                {
                    return; // User cancelled
                }

                // Write file
                File.WriteAllBytes(savePath, assetData);

                EditorUtility.DisplayDialog("Success", $"Asset exported to:\n{savePath}", "OK");
                LibraryUtilities.Log($"Exported asset: {asset.name} to {savePath}");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to export asset: {ex.Message}", "OK");
                LibraryUtilities.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Delete the asset from the library.
        /// </summary>
        private static void DeleteAsset(AssetMetadata asset, AssetLibraryLoader loader, string libraryPath)
        {
            // Confirm deletion
            if (!EditorUtility.DisplayDialog(
                "Delete Asset",
                $"Are you sure you want to delete '{asset.name}' from the library?\n\nThis action cannot be undone.",
                "Delete",
                "Cancel"))
            {
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("Deleting Asset", $"Removing '{asset.name}' from library...", 0.5f);

                // Use LibraryEditor to delete
                if (LibraryEditor.DeleteAssetFromLibrary(libraryPath, asset.id))
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Success", $"Asset deleted: {asset.name}", "OK");
                    LibraryUtilities.Log($"Deleted asset: {asset.name}");

                    // Refresh the library window
                    var window = EditorWindow.GetWindow<AssetLibraryWindow>();
                    if (window != null)
                    {
                        window.Repaint();
                    }
                }
                else
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Error", "Failed to delete asset from library.", "OK");
                }
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", $"Failed to delete asset: {ex.Message}", "OK");
                LibraryUtilities.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Show asset properties in a read-only dialog.
        /// </summary>
        private static void ShowAssetProperties(AssetMetadata asset)
        {
            AssetPropertiesDialog.ShowDialog(asset);
        }

        /// <summary>
        /// Copy the asset's relative path to clipboard.
        /// </summary>
        private static void CopyAssetPath(AssetMetadata asset)
        {
            GUIUtility.systemCopyBuffer = asset.relativePath;
            LibraryUtilities.Log($"Copied asset path to clipboard: {asset.relativePath}");
        }

        /// <summary>
        /// Reveal the library file in the OS file browser.
        /// </summary>
        private static void RevealLibraryFile(string libraryPath)
        {
            try
            {
                if (!File.Exists(libraryPath))
                {
                    EditorUtility.DisplayDialog("Error", $"Library file not found: {libraryPath}", "OK");
                    return;
                }

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    Process.Start("explorer.exe", "/select,\"" + libraryPath + "\"");
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    Process.Start("open", "-R \"" + libraryPath + "\"");
                }
                else if (Application.platform == RuntimePlatform.LinuxEditor)
                {
                    var directory = Path.GetDirectoryName(libraryPath);
                    Process.Start("xdg-open", "\"" + directory + "\"");
                }
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to reveal library file: {ex.Message}", "OK");
                LibraryUtilities.LogError(ex.Message);
            }
        }
    }
}
