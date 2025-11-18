using System;
using System.Collections.Generic;
using UnityEngine;

namespace CPAM
{
    /// <summary>
    /// Serializable data class representing a single asset in the library.
    /// </summary>
    [System.Serializable]
    public class AssetMetadata
    {
        /// <summary>
        /// Unique identifier for this asset (GUID format).
        /// </summary>
        [SerializeField]
        public string id = System.Guid.NewGuid().ToString();

        /// <summary>
        /// Human-readable name of the asset.
        /// </summary>
        [SerializeField]
        public string name = "";

        /// <summary>
        /// Relative path within the library ZIP structure (e.g., "assets/textures/WhiteDot.png").
        /// </summary>
        [SerializeField]
        public string relativePath = "";

        /// <summary>
        /// Asset type (e.g., "Texture2D", "GameObject", "MonoScript", "Material").
        /// </summary>
        [SerializeField]
        public string type = "";

        /// <summary>
        /// Group or category this asset belongs to (e.g., "UI Elements", "Player Systems").
        /// </summary>
        [SerializeField]
        public string group = "";

        /// <summary>
        /// List of tags for filtering and organization.
        /// </summary>
        [SerializeField]
        public List<string> tags = new List<string>();

        /// <summary>
        /// User-provided description of the asset.
        /// </summary>
        [SerializeField]
        public string description = "";

        /// <summary>
        /// Relative path to the thumbnail image within the library (e.g., "thumbnails/MyAsset.png" or "assets/textures/MyAsset.png").
        /// </summary>
        [SerializeField]
        public string thumbnailPath = "";

        /// <summary>
        /// File size in bytes (optional, for reference).
        /// </summary>
        [SerializeField]
        public long fileSize = 0;

        /// <summary>
        /// Timestamp when this asset was added to the library (ISO 8601 format).
        /// </summary>
        [SerializeField]
        public string dateAdded = "";

        public AssetMetadata()
        {
        }

        public AssetMetadata(string name, string type, string relativePath)
        {
            this.id = System.Guid.NewGuid().ToString();
            this.name = name;
            this.type = type;
            this.relativePath = relativePath;
            this.dateAdded = System.DateTime.UtcNow.ToString("O");
        }
    }
}
