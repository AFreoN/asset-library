using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace CPAM
{
    /// <summary>
    /// Provides methods for safely editing and modifying library contents.
    /// Handles operations like deleting assets, updating metadata, etc.
    /// </summary>
    public static class LibraryEditor
    {
        /// <summary>
        /// Delete an asset from the library by its ID.
        /// </summary>
        /// <param name="libraryPath">Path to the .unitylib file</param>
        /// <param name="assetId">ID of the asset to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public static bool DeleteAssetFromLibrary(string libraryPath, string assetId)
        {
            string extractedPath = null;

            try
            {
                if (!LibraryUtilities.IsValidLibraryFile(libraryPath))
                {
                    LibraryUtilities.LogError($"Invalid library file: {libraryPath}");
                    return false;
                }

                // Extract library
                extractedPath = UnityLibFileHandler.ExtractLibrary(libraryPath);
                if (string.IsNullOrEmpty(extractedPath))
                {
                    LibraryUtilities.LogError("Failed to extract library for editing");
                    return false;
                }

                // Read manifest
                var manifest = UnityLibFileHandler.ReadManifest(extractedPath);
                if (manifest == null)
                {
                    LibraryUtilities.LogError("Failed to read library manifest");
                    return false;
                }

                // Find the asset in the manifest
                var assetToDelete = manifest.assets.FirstOrDefault(a => a.id == assetId);
                if (assetToDelete == null)
                {
                    LibraryUtilities.LogError($"Asset with ID '{assetId}' not found in library");
                    return false;
                }

                // Delete asset file from extracted library
                var assetFilePath = Path.Combine(extractedPath, assetToDelete.relativePath);
                if (File.Exists(assetFilePath))
                {
                    try
                    {
                        File.Delete(assetFilePath);
                        LibraryUtilities.Log($"Deleted asset file: {assetToDelete.relativePath}");
                    }
                    catch (Exception ex)
                    {
                        LibraryUtilities.LogWarning($"Failed to delete asset file: {ex.Message}");
                    }
                }

                // Delete thumbnail if it exists
                if (!string.IsNullOrEmpty(assetToDelete.thumbnailPath))
                {
                    var thumbnailPath = Path.Combine(extractedPath, assetToDelete.thumbnailPath);
                    if (File.Exists(thumbnailPath))
                    {
                        try
                        {
                            File.Delete(thumbnailPath);
                            LibraryUtilities.Log($"Deleted thumbnail: {assetToDelete.thumbnailPath}");
                        }
                        catch (Exception ex)
                        {
                            LibraryUtilities.LogWarning($"Failed to delete thumbnail: {ex.Message}");
                        }
                    }
                }

                // Remove asset from manifest
                manifest.assets.Remove(assetToDelete);

                // Update manifest
                if (!UnityLibFileHandler.WriteManifest(extractedPath, manifest))
                {
                    LibraryUtilities.LogError("Failed to update manifest after deletion");
                    return false;
                }

                // Recompress library
                if (!UnityLibFileHandler.CompressLibrary(extractedPath, libraryPath))
                {
                    LibraryUtilities.LogError("Failed to recompress library after deletion");
                    return false;
                }

                LibraryUtilities.Log($"Successfully deleted asset '{assetToDelete.name}' (ID: {assetId}) from library");
                return true;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Unexpected error during asset deletion: {ex.Message}");
                return false;
            }
            finally
            {
                // Clean up extracted directory
                if (!string.IsNullOrEmpty(extractedPath) && Directory.Exists(extractedPath))
                {
                    UnityLibFileHandler.DeleteTemporaryDirectory(extractedPath);
                }
            }
        }

        /// <summary>
        /// Update asset metadata in the library.
        /// </summary>
        /// <param name="libraryPath">Path to the .unitylib file</param>
        /// <param name="assetId">ID of the asset to update</param>
        /// <param name="updatedMetadata">The updated metadata</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public static bool UpdateAssetMetadata(string libraryPath, string assetId, AssetMetadata updatedMetadata)
        {
            string extractedPath = null;

            try
            {
                if (!LibraryUtilities.IsValidLibraryFile(libraryPath))
                {
                    LibraryUtilities.LogError($"Invalid library file: {libraryPath}");
                    return false;
                }

                // Extract library
                extractedPath = UnityLibFileHandler.ExtractLibrary(libraryPath);
                if (string.IsNullOrEmpty(extractedPath))
                {
                    LibraryUtilities.LogError("Failed to extract library for editing");
                    return false;
                }

                // Read manifest
                var manifest = UnityLibFileHandler.ReadManifest(extractedPath);
                if (manifest == null)
                {
                    LibraryUtilities.LogError("Failed to read library manifest");
                    return false;
                }

                // Find and update the asset
                var assetIndex = manifest.assets.FindIndex(a => a.id == assetId);
                if (assetIndex < 0)
                {
                    LibraryUtilities.LogError($"Asset with ID '{assetId}' not found in library");
                    return false;
                }

                // Preserve file-related properties that shouldn't change
                updatedMetadata.id = assetId;
                updatedMetadata.relativePath = manifest.assets[assetIndex].relativePath;
                updatedMetadata.fileSize = manifest.assets[assetIndex].fileSize;
                updatedMetadata.dateAdded = manifest.assets[assetIndex].dateAdded;

                // Update the asset in manifest
                manifest.assets[assetIndex] = updatedMetadata;

                // Write updated manifest
                if (!UnityLibFileHandler.WriteManifest(extractedPath, manifest))
                {
                    LibraryUtilities.LogError("Failed to write updated manifest");
                    return false;
                }

                // Recompress library
                if (!UnityLibFileHandler.CompressLibrary(extractedPath, libraryPath))
                {
                    LibraryUtilities.LogError("Failed to recompress library after metadata update");
                    return false;
                }

                LibraryUtilities.Log($"Successfully updated metadata for asset '{updatedMetadata.name}' (ID: {assetId})");
                return true;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Unexpected error during metadata update: {ex.Message}");
                return false;
            }
            finally
            {
                // Clean up extracted directory
                if (!string.IsNullOrEmpty(extractedPath) && Directory.Exists(extractedPath))
                {
                    UnityLibFileHandler.DeleteTemporaryDirectory(extractedPath);
                }
            }
        }

        /// <summary>
        /// Rename an asset in the library.
        /// </summary>
        /// <param name="libraryPath">Path to the .unitylib file</param>
        /// <param name="assetId">ID of the asset to rename</param>
        /// <param name="newName">The new name for the asset</param>
        /// <returns>True if rename was successful, false otherwise</returns>
        public static bool RenameAsset(string libraryPath, string assetId, string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                LibraryUtilities.LogError("New asset name cannot be empty");
                return false;
            }

            string extractedPath = null;

            try
            {
                if (!LibraryUtilities.IsValidLibraryFile(libraryPath))
                {
                    LibraryUtilities.LogError($"Invalid library file: {libraryPath}");
                    return false;
                }

                // Extract library
                extractedPath = UnityLibFileHandler.ExtractLibrary(libraryPath);
                if (string.IsNullOrEmpty(extractedPath))
                {
                    LibraryUtilities.LogError("Failed to extract library for editing");
                    return false;
                }

                // Read manifest
                var manifest = UnityLibFileHandler.ReadManifest(extractedPath);
                if (manifest == null)
                {
                    LibraryUtilities.LogError("Failed to read library manifest");
                    return false;
                }

                // Find and rename the asset
                var asset = manifest.assets.FirstOrDefault(a => a.id == assetId);
                if (asset == null)
                {
                    LibraryUtilities.LogError($"Asset with ID '{assetId}' not found in library");
                    return false;
                }

                var oldName = asset.name;
                asset.name = newName;

                // Write updated manifest
                if (!UnityLibFileHandler.WriteManifest(extractedPath, manifest))
                {
                    LibraryUtilities.LogError("Failed to write updated manifest");
                    return false;
                }

                // Recompress library
                if (!UnityLibFileHandler.CompressLibrary(extractedPath, libraryPath))
                {
                    LibraryUtilities.LogError("Failed to recompress library after rename");
                    return false;
                }

                LibraryUtilities.Log($"Successfully renamed asset from '{oldName}' to '{newName}' (ID: {assetId})");
                return true;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Unexpected error during asset rename: {ex.Message}");
                return false;
            }
            finally
            {
                // Clean up extracted directory
                if (!string.IsNullOrEmpty(extractedPath) && Directory.Exists(extractedPath))
                {
                    UnityLibFileHandler.DeleteTemporaryDirectory(extractedPath);
                }
            }
        }
    }
}
