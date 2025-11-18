using UnityEditor;
using UnityEngine;

namespace CPAM
{
    /// <summary>
    /// Modal dialog for editing asset metadata in a library.
    /// Allows updating group, tags, and description.
    /// </summary>
    public class EditAssetMetadataDialog : EditorWindow
    {
        private static EditAssetMetadataDialog _instance;
        private AssetMetadata _asset;
        private AssetLibraryLoader _loader;
        private string _libraryPath;

        // UI state
        private string _name = "";
        private string _group = "";
        private string _tags = "";
        private string _description = "";
        private Vector2 _scrollPosition = Vector2.zero;

        private const float WindowWidth = 500f;
        private const float WindowHeight = 600f;

        /// <summary>
        /// Show the dialog for editing asset metadata.
        /// </summary>
        public static void ShowDialog(AssetMetadata asset, AssetLibraryLoader loader, string libraryPath)
        {
            if (asset == null || loader == null || string.IsNullOrEmpty(libraryPath))
            {
                EditorUtility.DisplayDialog("Error", "Invalid parameters for metadata editing.", "OK");
                return;
            }

            // Close existing dialog if any
            if (_instance != null)
            {
                _instance.Close();
            }

            // Create new dialog
            _instance = CreateInstance<EditAssetMetadataDialog>();
            _instance._asset = asset;
            _instance._loader = loader;
            _instance._libraryPath = libraryPath;

            // Copy asset data to editable fields
            _instance._name = asset.name;
            _instance._group = asset.group ?? "";
            _instance._tags = asset.tags != null ? string.Join(", ", asset.tags) : "";
            _instance._description = asset.description ?? "";

            _instance.minSize = new Vector2(WindowWidth, 400);
            _instance.maxSize = new Vector2(WindowWidth + 100, 800);
            _instance.titleContent = new GUIContent("Edit Asset Metadata");

            // Center the window
            var rect = EditorGUIUtility.GetMainWindowPosition();
            var x = (rect.width - WindowWidth) / 2 + rect.x;
            var y = (rect.height - WindowHeight) / 2 + rect.y;
            _instance.position = new Rect(x, y, WindowWidth, WindowHeight);

            _instance.ShowModal();
        }

        private void OnGUI()
        {
            if (_asset == null)
            {
                EditorGUILayout.HelpBox("Asset is null.", MessageType.Error);
                return;
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            // Title
            EditorGUILayout.LabelField("Edit Asset Metadata", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Asset info
            EditorGUILayout.LabelField("Asset Information", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.TextField("Asset ID:", _asset.id);
            EditorGUILayout.TextField("Type:", _asset.type);
            EditorGUILayout.TextField("Relative Path:", _asset.relativePath);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Editable fields
            EditorGUILayout.LabelField("Editable Metadata", EditorStyles.boldLabel);

            _name = EditorGUILayout.TextField("Asset Name:", _name);
            EditorGUILayout.HelpBox("Changing the name will rename the asset in the library.", MessageType.Info);
            EditorGUILayout.Space();

            _group = EditorGUILayout.TextField("Group/Category:", _group);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Tags (comma-separated):");
            _tags = EditorGUILayout.TextArea(_tags, GUILayout.Height(40));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Description:");
            _description = EditorGUILayout.TextArea(_description, GUILayout.Height(80));

            GUILayout.FlexibleSpace();

            // Buttons
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                Close();
            }

            if (GUILayout.Button("Save Changes", GUILayout.Height(30)))
            {
                SaveChanges();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        private void SaveChanges()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Saving Metadata", "Updating asset metadata...", 0.5f);

                bool needsRename = _name != _asset.name;

                // Rename if necessary
                if (needsRename)
                {
                    if (!LibraryEditor.RenameAsset(_libraryPath, _asset.id, _name))
                    {
                        EditorUtility.ClearProgressBar();
                        EditorUtility.DisplayDialog("Error", "Failed to rename asset. Check the console for details.", "OK");
                        return;
                    }
                }

                // Update metadata
                _asset.name = _name;
                _asset.group = string.IsNullOrEmpty(_group) ? null : _group;
                _asset.tags = ParseTags(_tags);
                _asset.description = string.IsNullOrEmpty(_description) ? null : _description;

                // Update in library
                if (!LibraryEditor.UpdateAssetMetadata(_libraryPath, _asset.id, _asset))
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Error", "Failed to save metadata. Check the console for details.", "OK");
                    return;
                }

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Success", "Asset metadata updated successfully!", "OK");

                // Refresh the library loader to reflect changes
                if (_loader != null && !string.IsNullOrEmpty(_libraryPath))
                {
                    _loader.UnloadLibrary();
                    _loader.LoadLibrary(_libraryPath);
                }

                Close();
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", $"Unexpected error: {ex.Message}", "OK");
                LibraryUtilities.LogError(ex.Message);
            }
        }

        private System.Collections.Generic.List<string> ParseTags(string tagsString)
        {
            var tags = new System.Collections.Generic.List<string>();

            if (string.IsNullOrEmpty(tagsString))
            {
                return tags;
            }

            foreach (var tag in tagsString.Split(','))
            {
                var trimmedTag = tag.Trim();
                if (!string.IsNullOrEmpty(trimmedTag))
                {
                    tags.Add(trimmedTag);
                }
            }

            return tags;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
