using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CPAL
{
    /// <summary>
    /// Utility class providing helper functions for library operations.
    /// </summary>
    public static class LibraryUtilities
    {
        /// <summary>
        /// Map of asset types to their category folders.
        /// </summary>
        private static readonly Dictionary<string, string> AssetTypeToCategory = new Dictionary<string, string>
        {
            // Textures
            { "Texture2D", "textures" },
            { "Sprite", "textures" },
            { "RenderTexture", "textures" },

            // Prefabs
            { "GameObject", "prefabs" },

            // Scripts
            { "MonoScript", "scripts" },

            // Materials
            { "Material", "materials" },

            // Audio
            { "AudioClip", "audio" },

            // Models
            { "Mesh", "models" },
            { "SkinnedMesh", "models" },

            // Animations
            { "AnimationClip", "animations" },
            { "Animator", "animations" },
            { "AnimatorController", "animations" },

            // Shaders
            { "Shader", "shaders" },
            { "ComputeShader", "shaders" },

            // Scenes
            { "SceneAsset", "scenes" },

            // ScriptableObjects
            { "ScriptableObject", "scriptable" }
        };

        /// <summary>
        /// Determine the category folder for an asset based on its type.
        /// </summary>
        public static string GetAssetCategory(string assetType)
        {
            if (string.IsNullOrEmpty(assetType))
            {
                return "other";
            }

            if (AssetTypeToCategory.TryGetValue(assetType, out var category))
            {
                return category;
            }

            return "other";
        }

        /// <summary>
        /// Generate a unique GUID string (for asset IDs).
        /// </summary>
        public static string GenerateUniqueId()
        {
            return System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Combine path components in a cross-platform way.
        /// Always uses forward slashes for paths within ZIP archives.
        /// </summary>
        public static string CombinePaths(params string[] pathComponents)
        {
            return Path.Combine(pathComponents).Replace(Path.DirectorySeparatorChar, '/');
        }

        /// <summary>
        /// Combine path components, using forward slashes (for ZIP paths).
        /// </summary>
        public static string CombineZipPaths(params string[] pathComponents)
        {
            return string.Join("/", pathComponents);
        }

        /// <summary>
        /// Validate that a file exists and is readable.
        /// </summary>
        public static bool IsFileValid(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            return File.Exists(filePath);
        }

        /// <summary>
        /// Get the file extension without the dot.
        /// </summary>
        public static string GetFileExtension(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return "";
            }

            var ext = Path.GetExtension(filePath);
            return ext.StartsWith(".") ? ext.Substring(1) : ext;
        }

        /// <summary>
        /// Get the filename without extension.
        /// </summary>
        public static string GetFileNameWithoutExtension(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>
        /// Get the filename including extension.
        /// </summary>
        public static string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        /// <summary>
        /// Get the directory of a file path.
        /// </summary>
        public static string GetDirectory(string filePath)
        {
            return Path.GetDirectoryName(filePath);
        }

        /// <summary>
        /// Validate a library file has .unitylib extension.
        /// </summary>
        public static bool IsValidLibraryFile(string filePath)
        {
            if (!IsFileValid(filePath))
            {
                return false;
            }

            var extension = GetFileExtension(filePath);
            return extension.Equals("unitylib", System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sanitize a filename to remove invalid characters.
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "unnamed";
            }

            var invalidChars = new string(Path.GetInvalidFileNameChars());
            var result = fileName;

            foreach (var c in invalidChars)
            {
                result = result.Replace(c, '_');
            }

            return result;
        }

        /// <summary>
        /// Get a temporary directory for extracting libraries.
        /// </summary>
        public static string GetLibraryCacheDirectory()
        {
            var cachePath = Path.Combine(Application.temporaryCachePath, "CPAL", "LibraryCache");
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            return cachePath;
        }

        /// <summary>
        /// Create a unique temporary directory for a library extraction.
        /// </summary>
        public static string CreateLibraryTempDirectory(string libraryName)
        {
            var basePath = GetLibraryCacheDirectory();
            var sanitizedName = SanitizeFileName(libraryName);
            var uniquePath = Path.Combine(basePath, sanitizedName + "_" + System.DateTime.Now.Ticks);

            if (!Directory.Exists(uniquePath))
            {
                Directory.CreateDirectory(uniquePath);
            }

            return uniquePath;
        }

        /// <summary>
        /// Log an error message to Unity console.
        /// </summary>
        public static void LogError(string message)
        {
            Debug.LogError($"[CPAL] {message}");
        }

        /// <summary>
        /// Log a warning message to Unity console.
        /// </summary>
        public static void LogWarning(string message)
        {
            Debug.LogWarning($"[CPAL] {message}");
        }

        /// <summary>
        /// Log an info message to Unity console.
        /// </summary>
        public static void Log(string message)
        {
            Debug.Log($"[CPAL] {message}");
        }

        /// <summary>
        /// Format file size in human-readable format (KB, MB, etc).
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            if (bytes >= GB)
            {
                return $"{bytes / (double)GB:F2} GB";
            }
            else if (bytes >= MB)
            {
                return $"{bytes / (double)MB:F2} MB";
            }
            else if (bytes >= KB)
            {
                return $"{bytes / (double)KB:F2} KB";
            }
            else
            {
                return $"{bytes} B";
            }
        }
    }
}
