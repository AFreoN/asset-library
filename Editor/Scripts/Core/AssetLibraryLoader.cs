using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CPAM
{
    /// <summary>
    /// Handles loading and caching of asset libraries.
    /// Manages extraction, manifest parsing, and asset access.
    /// </summary>
    public class AssetLibraryLoader
    {
        /// <summary>
        /// The file path to the library being loaded.
        /// </summary>
        public string LibraryPath { get; private set; }

        /// <summary>
        /// The path to the extracted library contents in the temporary cache.
        /// </summary>
        public string ExtractedPath { get; private set; }

        /// <summary>
        /// The loaded manifest.
        /// </summary>
        public LibraryManifest Manifest { get; private set; }

        /// <summary>
        /// Whether the library is currently loaded and available.
        /// </summary>
        public bool IsLoaded { get; private set; }

        private bool _disposed = false;

        public AssetLibraryLoader()
        {
        }

        /// <summary>
        /// Load a library from a .unitylib file.
        /// </summary>
        public bool LoadLibrary(string libraryPath)
        {
            try
            {
                // Clean up previous library if any
                if (IsLoaded)
                {
                    UnloadLibrary();
                }

                // Validate library file
                if (!LibraryUtilities.IsValidLibraryFile(libraryPath))
                {
                    LibraryUtilities.LogError($"Invalid library file: {libraryPath}");
                    return false;
                }

                // Extract library
                ExtractedPath = UnityLibFileHandler.ExtractLibrary(libraryPath);
                if (string.IsNullOrEmpty(ExtractedPath))
                {
                    LibraryUtilities.LogError("Failed to extract library");
                    return false;
                }

                // Read manifest
                Manifest = UnityLibFileHandler.ReadManifest(ExtractedPath);
                if (Manifest == null)
                {
                    LibraryUtilities.LogError("Failed to read manifest");
                    UnityLibFileHandler.DeleteTemporaryDirectory(ExtractedPath);
                    ExtractedPath = null;
                    return false;
                }

                LibraryPath = libraryPath;
                IsLoaded = true;

                LibraryUtilities.Log($"Loaded library: {Manifest.libraryName} ({Manifest.GetAssetCount()} assets)");
                return true;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Unexpected error loading library: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unload the current library and clean up resources.
        /// </summary>
        public void UnloadLibrary()
        {
            try
            {
                if (!string.IsNullOrEmpty(ExtractedPath) && Directory.Exists(ExtractedPath))
                {
                    UnityLibFileHandler.DeleteTemporaryDirectory(ExtractedPath);
                }

                LibraryPath = null;
                ExtractedPath = null;
                Manifest = null;
                IsLoaded = false;

                LibraryUtilities.Log("Library unloaded");
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogWarning($"Error while unloading library: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all assets in the library.
        /// </summary>
        public List<AssetMetadata> GetAllAssets()
        {
            if (!IsLoaded || Manifest == null)
            {
                return new List<AssetMetadata>();
            }

            return new List<AssetMetadata>(Manifest.assets);
        }

        /// <summary>
        /// Get an asset by ID.
        /// </summary>
        public AssetMetadata GetAssetById(string id)
        {
            if (!IsLoaded || Manifest == null)
            {
                return null;
            }

            return Manifest.FindAssetById(id);
        }

        /// <summary>
        /// Search assets by name (case-insensitive).
        /// </summary>
        public List<AssetMetadata> SearchAssetsByName(string searchTerm)
        {
            if (!IsLoaded || Manifest == null || string.IsNullOrEmpty(searchTerm))
            {
                return GetAllAssets();
            }

            var searchLower = searchTerm.ToLower();
            return Manifest.assets
                .Where(a => a.name.ToLower().Contains(searchLower))
                .ToList();
        }

        /// <summary>
        /// Filter assets by type.
        /// </summary>
        public List<AssetMetadata> GetAssetsByType(string type)
        {
            if (!IsLoaded || Manifest == null || string.IsNullOrEmpty(type))
            {
                return GetAllAssets();
            }

            return Manifest.assets
                .Where(a => a.type.Equals(type, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Filter assets by tag (asset must have the tag).
        /// </summary>
        public List<AssetMetadata> GetAssetsByTag(string tag)
        {
            if (!IsLoaded || Manifest == null || string.IsNullOrEmpty(tag))
            {
                return GetAllAssets();
            }

            var tagLower = tag.ToLower();
            return Manifest.assets
                .Where(a => a.tags.Any(t => t.ToLower().Equals(tagLower)))
                .ToList();
        }

        /// <summary>
        /// Filter assets by group/category.
        /// </summary>
        public List<AssetMetadata> GetAssetsByGroup(string group)
        {
            if (!IsLoaded || Manifest == null || string.IsNullOrEmpty(group))
            {
                return GetAllAssets();
            }

            return Manifest.assets
                .Where(a => a.group.Equals(group, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Get all unique asset types in the library.
        /// </summary>
        public List<string> GetAssetTypes()
        {
            if (!IsLoaded || Manifest == null)
            {
                return new List<string>();
            }

            return Manifest.GetUniqueAssetTypes();
        }

        /// <summary>
        /// Get all unique tags in the library.
        /// </summary>
        public List<string> GetTags()
        {
            if (!IsLoaded || Manifest == null)
            {
                return new List<string>();
            }

            return Manifest.GetUniqueTags();
        }

        /// <summary>
        /// Get all unique groups in the library.
        /// </summary>
        public List<string> GetGroups()
        {
            if (!IsLoaded || Manifest == null)
            {
                return new List<string>();
            }

            return Manifest.GetUniqueGroups();
        }

        /// <summary>
        /// Get the thumbnail image bytes for an asset.
        /// Returns null if thumbnail doesn't exist.
        /// </summary>
        public byte[] GetAssetThumbnail(AssetMetadata asset)
        {
            if (!IsLoaded || asset == null || string.IsNullOrEmpty(asset.thumbnailPath))
            {
                return null;
            }

            return UnityLibFileHandler.ReadFileFromLibrary(ExtractedPath, asset.thumbnailPath);
        }

        /// <summary>
        /// Get the file bytes for an asset from the library.
        /// </summary>
        public byte[] GetAssetFile(AssetMetadata asset)
        {
            if (!IsLoaded || asset == null || string.IsNullOrEmpty(asset.relativePath))
            {
                return null;
            }

            return UnityLibFileHandler.ReadFileFromLibrary(ExtractedPath, asset.relativePath);
        }

        /// <summary>
        /// Get library information.
        /// </summary>
        public LibraryInfo GetLibraryInfo()
        {
            if (!IsLoaded || Manifest == null)
            {
                return null;
            }

            return new LibraryInfo
            {
                Name = Manifest.libraryName,
                Version = Manifest.version,
                AssetCount = Manifest.GetAssetCount(),
                CreatedDate = Manifest.createdDate,
                LastModifiedDate = Manifest.lastModifiedDate
            };
        }

        /// <summary>
        /// Cleanup resources when the loader is disposed.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                UnloadLibrary();
                _disposed = true;
            }
        }

        ~AssetLibraryLoader()
        {
            Dispose();
        }
    }

    /// <summary>
    /// Simple data class for library information.
    /// </summary>
    public class LibraryInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public int AssetCount { get; set; }
        public string CreatedDate { get; set; }
        public string LastModifiedDate { get; set; }
    }
}
