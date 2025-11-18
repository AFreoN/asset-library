using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CPAM
{
    /// <summary>
    /// Manages recently used libraries with metadata caching.
    /// Handles persistence through EditorPrefs and provides MRU (Most Recently Used) ordering.
    /// Shared singleton instance used across main window and dialogs.
    /// </summary>
    public class RecentLibrariesManager
    {
        private const string RECENT_LIBRARIES_KEY = "CPAM.RecentLibrariesList";
        private const string METADATA_CACHE_KEY = "CPAM.LibraryMetadataCache";
        private const string LEGACY_LAST_LIBRARY_KEY = "CPAM.LastLibraryPath";

        [System.Serializable]
        public class LibraryMetadata
        {
            public string path;
            public string libraryName;
            public int assetCount;
            public string lastModified;
            public string lastAccessed;
        }

        [System.Serializable]
        private class LibraryMetadataList
        {
            public LibraryMetadata[] items = new LibraryMetadata[0];
        }

        [System.Serializable]
        private class RecentLibraryList
        {
            public string[] paths = new string[0];
        }

        private static RecentLibrariesManager _instance;
        private List<string> _recentLibraries = new List<string>();
        private Dictionary<string, LibraryMetadata> _metadataCache = new Dictionary<string, LibraryMetadata>();
        private bool _isDirty = false;

        public static RecentLibrariesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RecentLibrariesManager();
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        private RecentLibrariesManager()
        {
        }

        /// <summary>
        /// Initialize the manager - loads recent libraries from EditorPrefs.
        /// </summary>
        private void Initialize()
        {
            LoadFromEditorPrefs();
            LoadMetadataCache();
        }

        /// <summary>
        /// Load recent libraries list from EditorPrefs, with migration from legacy key.
        /// </summary>
        private void LoadFromEditorPrefs()
        {
            _recentLibraries.Clear();

            // Try to load new format
            string json = EditorPrefs.GetString(RECENT_LIBRARIES_KEY, "");

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    RecentLibraryList list = JsonUtility.FromJson<RecentLibraryList>(json);
                    if (list?.paths != null)
                    {
                        _recentLibraries = new List<string>(list.paths);
                    }
                }
                catch (Exception ex)
                {
                    LibraryUtilities.LogWarning($"Failed to load recent libraries list: {ex.Message}");
                }
            }
            else
            {
                // Try to migrate from legacy key
                string legacyPath = EditorPrefs.GetString(LEGACY_LAST_LIBRARY_KEY, "");
                if (!string.IsNullOrEmpty(legacyPath))
                {
                    _recentLibraries.Add(legacyPath);
                    _isDirty = true;
                    LibraryUtilities.Log("Migrated legacy library path to new recent libraries system");
                }
            }

            // Validate all paths - remove ones that don't exist
            ValidatePaths();
        }

        /// <summary>
        /// Load metadata cache from EditorPrefs.
        /// </summary>
        private void LoadMetadataCache()
        {
            _metadataCache.Clear();

            string json = EditorPrefs.GetString(METADATA_CACHE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    LibraryMetadataList list = JsonUtility.FromJson<LibraryMetadataList>(json);
                    if (list?.items != null)
                    {
                        foreach (var item in list.items)
                        {
                            if (item != null && !string.IsNullOrEmpty(item.path))
                            {
                                _metadataCache[item.path] = item;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LibraryUtilities.LogWarning($"Failed to load metadata cache: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Remove paths that no longer exist or are invalid.
        /// </summary>
        private void ValidatePaths()
        {
            List<string> validPaths = new List<string>();

            foreach (var path in _recentLibraries)
            {
                if (!string.IsNullOrEmpty(path) && LibraryUtilities.IsValidLibraryFile(path))
                {
                    validPaths.Add(path);
                }
            }

            if (validPaths.Count != _recentLibraries.Count)
            {
                _isDirty = true;
                _recentLibraries = validPaths;
            }
        }

        /// <summary>
        /// Get all recent libraries sorted by recency (most recent first).
        /// </summary>
        public List<LibraryMetadata> GetRecentLibraries()
        {
            ValidatePaths();

            List<LibraryMetadata> result = new List<LibraryMetadata>();

            foreach (var path in _recentLibraries)
            {
                if (_metadataCache.TryGetValue(path, out var metadata))
                {
                    result.Add(metadata);
                }
                else
                {
                    // Create minimal metadata for uncached path
                    result.Add(new LibraryMetadata
                    {
                        path = path,
                        libraryName = LibraryUtilities.GetFileName(path),
                        assetCount = 0,
                        lastModified = "",
                        lastAccessed = DateTime.UtcNow.ToString("O")
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Register a library as recently used - moves to top of list if already present.
        /// Automatically updates metadata by reading the library's manifest.
        /// </summary>
        public void RegisterLibraryUsage(string libraryPath)
        {
            if (string.IsNullOrEmpty(libraryPath) || !LibraryUtilities.IsValidLibraryFile(libraryPath))
            {
                return;
            }

            // Remove if already in list
            _recentLibraries.Remove(libraryPath);

            // Add to front (most recent)
            _recentLibraries.Insert(0, libraryPath);

            // Update metadata
            UpdateMetadata(libraryPath);

            _isDirty = true;
            SaveToEditorPrefs();
        }

        /// <summary>
        /// Update metadata for a library by reading its manifest.
        /// </summary>
        private void UpdateMetadata(string libraryPath)
        {
            try
            {
                string extractPath = UnityLibFileHandler.ExtractLibrary(libraryPath);
                if (string.IsNullOrEmpty(extractPath))
                {
                    LibraryUtilities.LogWarning($"Failed to extract library for metadata: {libraryPath}");
                    return;
                }

                LibraryManifest manifest = UnityLibFileHandler.ReadManifest(extractPath);
                UnityLibFileHandler.DeleteTemporaryDirectory(extractPath);

                if (manifest == null)
                {
                    return;
                }

                var metadata = new LibraryMetadata
                {
                    path = libraryPath,
                    libraryName = manifest.libraryName,
                    assetCount = manifest.assets?.Count ?? 0,
                    lastModified = manifest.lastModifiedDate,
                    lastAccessed = DateTime.UtcNow.ToString("O")
                };

                _metadataCache[libraryPath] = metadata;
                SaveMetadataCache();
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogWarning($"Failed to update metadata for {libraryPath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove a library from the recent list.
        /// </summary>
        public void RemoveLibrary(string libraryPath)
        {
            if (_recentLibraries.Remove(libraryPath))
            {
                _metadataCache.Remove(libraryPath);
                _isDirty = true;
                SaveToEditorPrefs();
                SaveMetadataCache();
            }
        }

        /// <summary>
        /// Clear all recent libraries.
        /// </summary>
        public void ClearAll()
        {
            _recentLibraries.Clear();
            _metadataCache.Clear();
            EditorPrefs.DeleteKey(RECENT_LIBRARIES_KEY);
            EditorPrefs.DeleteKey(METADATA_CACHE_KEY);
            LibraryUtilities.Log("Recent libraries list cleared");
        }

        /// <summary>
        /// Save recent libraries list to EditorPrefs.
        /// </summary>
        private void SaveToEditorPrefs()
        {
            if (!_isDirty)
            {
                return;
            }

            try
            {
                RecentLibraryList list = new RecentLibraryList { paths = _recentLibraries.ToArray() };
                string json = JsonUtility.ToJson(list);
                EditorPrefs.SetString(RECENT_LIBRARIES_KEY, json);
                _isDirty = false;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to save recent libraries list: {ex.Message}");
            }
        }

        /// <summary>
        /// Save metadata cache to EditorPrefs.
        /// </summary>
        private void SaveMetadataCache()
        {
            try
            {
                List<LibraryMetadata> items = new List<LibraryMetadata>(_metadataCache.Values);
                LibraryMetadataList list = new LibraryMetadataList { items = items.ToArray() };
                string json = JsonUtility.ToJson(list);
                EditorPrefs.SetString(METADATA_CACHE_KEY, json);
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to save metadata cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Force save if dirty.
        /// </summary>
        public void Save()
        {
            if (_isDirty)
            {
                SaveToEditorPrefs();
            }
        }
    }
}
