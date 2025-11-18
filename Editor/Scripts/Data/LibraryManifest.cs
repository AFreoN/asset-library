using System;
using System.Collections.Generic;
using UnityEngine;

namespace CPAL
{
    /// <summary>
    /// Serializable data class representing the entire asset library manifest.
    /// This is stored as JSON within the .unitylib file.
    /// </summary>
    [System.Serializable]
    public class LibraryManifest
    {
        /// <summary>
        /// User-friendly name for this library.
        /// </summary>
        [SerializeField]
        public string libraryName = "My Asset Library";

        /// <summary>
        /// Semantic version of the library format (for compatibility).
        /// </summary>
        [SerializeField]
        public string version = "1.0.0";

        /// <summary>
        /// Timestamp when this library was created (ISO 8601 format).
        /// </summary>
        [SerializeField]
        public string createdDate = "";

        /// <summary>
        /// Timestamp when this library was last modified (ISO 8601 format).
        /// </summary>
        [SerializeField]
        public string lastModifiedDate = "";

        /// <summary>
        /// List of all assets in the library.
        /// </summary>
        [SerializeField]
        public List<AssetMetadata> assets = new List<AssetMetadata>();

        public LibraryManifest()
        {
            this.createdDate = System.DateTime.UtcNow.ToString("O");
            this.lastModifiedDate = System.DateTime.UtcNow.ToString("O");
        }

        public LibraryManifest(string libraryName) : this()
        {
            this.libraryName = libraryName;
        }

        /// <summary>
        /// Update the last modified timestamp to now.
        /// </summary>
        public void UpdateModifiedDate()
        {
            this.lastModifiedDate = System.DateTime.UtcNow.ToString("O");
        }

        /// <summary>
        /// Get the total count of assets in the library.
        /// </summary>
        public int GetAssetCount()
        {
            return assets.Count;
        }

        /// <summary>
        /// Find an asset by ID.
        /// </summary>
        public AssetMetadata FindAssetById(string id)
        {
            return assets.Find(a => a.id == id);
        }

        /// <summary>
        /// Get all unique asset types in the library.
        /// </summary>
        public List<string> GetUniqueAssetTypes()
        {
            var types = new HashSet<string>();
            foreach (var asset in assets)
            {
                if (!string.IsNullOrEmpty(asset.type))
                {
                    types.Add(asset.type);
                }
            }
            return new List<string>(types);
        }

        /// <summary>
        /// Get all unique tags in the library.
        /// </summary>
        public List<string> GetUniqueTags()
        {
            var tags = new HashSet<string>();
            foreach (var asset in assets)
            {
                foreach (var tag in asset.tags)
                {
                    if (!string.IsNullOrEmpty(tag))
                    {
                        tags.Add(tag);
                    }
                }
            }
            return new List<string>(tags);
        }

        /// <summary>
        /// Get all unique groups in the library.
        /// </summary>
        public List<string> GetUniqueGroups()
        {
            var groups = new HashSet<string>();
            foreach (var asset in assets)
            {
                if (!string.IsNullOrEmpty(asset.group))
                {
                    groups.Add(asset.group);
                }
            }
            return new List<string>(groups);
        }
    }
}
