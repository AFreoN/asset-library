using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CPAM
{
    /// <summary>
    /// Handles adding assets to an existing library.
    /// Manages extraction, asset copying, manifest updating, and recompression.
    /// </summary>
    public static class LibraryWriter
    {
        /// <summary>
        /// Information about an asset being added to the library.
        /// </summary>
        public class AssetAddRequest
        {
            public string SourcePath { get; set; }
            public string AssetName { get; set; }
            public string AssetType { get; set; }
            public string Group { get; set; }
            public List<string> Tags { get; set; } = new List<string>();
            public string Description { get; set; }
            public string CustomThumbnailPath { get; set; } // Optional custom thumbnail
        }

        /// <summary>
        /// Add one or more assets to a library.
        /// </summary>
        public static bool AddAssetsToLibrary(string libraryPath, List<AssetAddRequest> assetsToAdd)
        {
            if (!LibraryUtilities.IsValidLibraryFile(libraryPath))
            {
                LibraryUtilities.LogError($"Invalid library file: {libraryPath}");
                return false;
            }

            if (assetsToAdd == null || assetsToAdd.Count == 0)
            {
                LibraryUtilities.LogWarning("No assets to add");
                return false;
            }

            string extractedPath = null;
            try
            {
                // Extract library
                extractedPath = UnityLibFileHandler.ExtractLibrary(libraryPath);
                if (string.IsNullOrEmpty(extractedPath))
                {
                    LibraryUtilities.LogError("Failed to extract library for writing");
                    return false;
                }

                // Read existing manifest
                var manifest = UnityLibFileHandler.ReadManifest(extractedPath);
                if (manifest == null)
                {
                    LibraryUtilities.LogError("Failed to read manifest");
                    return false;
                }

                // Add each asset
                foreach (var request in assetsToAdd)
                {
                    if (!AddAssetToManifest(extractedPath, manifest, request))
                    {
                        LibraryUtilities.LogWarning($"Failed to add asset: {request.AssetName}");
                        continue;
                    }
                }

                // Write updated manifest
                if (!UnityLibFileHandler.WriteManifest(extractedPath, manifest))
                {
                    LibraryUtilities.LogError("Failed to write updated manifest");
                    return false;
                }

                // Recompress library
                if (!UnityLibFileHandler.CompressLibrary(extractedPath, libraryPath))
                {
                    LibraryUtilities.LogError("Failed to recompress library");
                    return false;
                }

                LibraryUtilities.Log($"Successfully added {assetsToAdd.Count} asset(s) to library");
                return true;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Unexpected error adding assets: {ex.Message}");
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
        /// Add a single asset to the library.
        /// </summary>
        private static bool AddAssetToManifest(string extractedLibraryPath, LibraryManifest manifest, AssetAddRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.SourcePath) || !File.Exists(request.SourcePath))
                {
                    LibraryUtilities.LogWarning($"Source file not found: {request.SourcePath}");
                    return false;
                }

                if (string.IsNullOrEmpty(request.AssetName))
                {
                    request.AssetName = Path.GetFileNameWithoutExtension(request.SourcePath);
                }

                // Determine category from type
                var category = LibraryUtilities.GetAssetCategory(request.AssetType);
                var fileName = Path.GetFileName(request.SourcePath);
                var assetRelativePath = Path.Combine(UnityLibFileHandler.AssetsFolderName, category, fileName)
                    .Replace(Path.DirectorySeparatorChar, '/');

                // Copy asset file to library
                if (!UnityLibFileHandler.WriteFileToLibrary(extractedLibraryPath, request.SourcePath, assetRelativePath))
                {
                    return false;
                }

                // Handle thumbnail
                string thumbnailPath = null;

                // For image types, use the asset itself as thumbnail
                if (request.AssetType == "Texture2D" || request.AssetType == "Sprite")
                {
                    thumbnailPath = assetRelativePath;
                }
                else if (!string.IsNullOrEmpty(request.CustomThumbnailPath) && File.Exists(request.CustomThumbnailPath))
                {
                    // Use custom thumbnail if provided
                    var thumbFileName = Path.GetFileName(request.CustomThumbnailPath);
                    var thumbRelativePath = Path.Combine(UnityLibFileHandler.ThumbnailsFolderName, thumbFileName)
                        .Replace(Path.DirectorySeparatorChar, '/');

                    if (UnityLibFileHandler.WriteFileToLibrary(extractedLibraryPath, request.CustomThumbnailPath, thumbRelativePath))
                    {
                        thumbnailPath = thumbRelativePath;
                    }
                }

                // Get file size
                var fileInfo = new FileInfo(request.SourcePath);
                var fileSize = fileInfo.Length;

                // Create asset metadata
                var assetMetadata = new AssetMetadata
                {
                    id = LibraryUtilities.GenerateUniqueId(),
                    name = request.AssetName,
                    relativePath = assetRelativePath,
                    type = request.AssetType,
                    group = request.Group ?? category,
                    tags = request.Tags ?? new List<string>(),
                    description = request.Description ?? "",
                    thumbnailPath = thumbnailPath ?? "",
                    fileSize = fileSize,
                    dateAdded = System.DateTime.UtcNow.ToString("O")
                };

                // Add to manifest
                manifest.assets.Add(assetMetadata);

                LibraryUtilities.Log($"Added asset to manifest: {request.AssetName}");
                return true;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to add asset to manifest: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create a new empty library.
        /// </summary>
        public static bool CreateNewLibrary(string libraryPath, string libraryName)
        {
            if (string.IsNullOrEmpty(libraryPath) || string.IsNullOrEmpty(libraryName))
            {
                LibraryUtilities.LogError("Library path and name cannot be empty");
                return false;
            }

            return UnityLibFileHandler.CreateNewLibrary(libraryPath, libraryName);
        }
    }
}
