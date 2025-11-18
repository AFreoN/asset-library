using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CPAM
{
    /// <summary>
    /// Modal dialog for adding assets to a library.
    /// Allows users to input metadata and select target library.
    /// </summary>
    public class AddAssetDialog : EditorWindow
    {
        private static AddAssetDialog _instance;
        private List<ContextMenuExtension.SelectedAsset> _selectedAssets;

        // UI state
        private string _libraryPath = "";
        private string _group = "";
        private string _tags = "";
        private string _description = "";
        private Object _customThumbnail;
        private Vector2 _scrollPosition = Vector2.zero;

        private const float WindowWidth = 500f;
        private const float WindowHeight = 600f;
        private const string LastLibraryPathKey = "CPAM.LastLibraryPathForAdd";

        private void OnEnable()
        {
            // Load last used library from EditorPrefs
            _libraryPath = EditorPrefs.GetString(LastLibraryPathKey, "");
        }

        /// <summary>
        /// Show the dialog with selected assets.
        /// </summary>
        public static void ShowDialog(List<ContextMenuExtension.SelectedAsset> selectedAssets)
        {
            // Close existing dialog if any
            if (_instance != null)
            {
                _instance.Close();
            }

            // Create new dialog
            _instance = CreateInstance<AddAssetDialog>();
            _instance._selectedAssets = selectedAssets;
            _instance.minSize = new Vector2(WindowWidth, 400);
            _instance.maxSize = new Vector2(WindowWidth + 100, 800);
            _instance.titleContent = new GUIContent("Add to Asset Library");

            // Center the window
            var rect = EditorGUIUtility.GetMainWindowPosition();
            var x = (rect.width - WindowWidth) / 2 + rect.x;
            var y = (rect.height - WindowHeight) / 2 + rect.y;
            _instance.position = new Rect(x, y, WindowWidth, WindowHeight);

            _instance.ShowModal();
        }

        private void OnGUI()
        {
            if (_selectedAssets == null || _selectedAssets.Count == 0)
            {
                EditorGUILayout.HelpBox("No assets selected.", MessageType.Error);
                return;
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            // Title
            EditorGUILayout.LabelField("Add Assets to Library", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Selected assets
            EditorGUILayout.LabelField("Selected Assets:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            foreach (var asset in _selectedAssets)
            {
                EditorGUILayout.LabelField($"○ {asset.Name} ({asset.Type})", EditorStyles.label);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Library file selection
            EditorGUILayout.LabelField("Target Library", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _libraryPath = EditorGUILayout.TextField("Library File:", _libraryPath);

            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                var path = EditorUtility.OpenFilePanel("Select Library", "", "unitylib");
                if (!string.IsNullOrEmpty(path))
                {
                    _libraryPath = path;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_libraryPath))
            {
                if (!LibraryUtilities.IsValidLibraryFile(_libraryPath))
                {
                    EditorGUILayout.HelpBox("Selected file is not a valid .unitylib file.", MessageType.Warning);
                }
            }

            EditorGUILayout.Space();

            // Metadata
            EditorGUILayout.LabelField("Metadata", EditorStyles.boldLabel);
            _group = EditorGUILayout.TextField("Group/Category:", _group);
            _tags = EditorGUILayout.TextField("Tags (comma-separated):", _tags);

            EditorGUILayout.LabelField("Description:");
            _description = EditorGUILayout.TextArea(_description, GUILayout.Height(80));

            EditorGUILayout.Space();

            // Custom thumbnail
            EditorGUILayout.LabelField("Custom Thumbnail (Optional)", EditorStyles.boldLabel);
            _customThumbnail = EditorGUILayout.ObjectField("Thumbnail:", _customThumbnail, typeof(Texture2D), false);
            EditorGUILayout.HelpBox("For image assets, the asset itself will be used as thumbnail.", MessageType.Info);

            GUILayout.FlexibleSpace();

            // Buttons
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                Close();
            }

            if (GUILayout.Button("Add to Library", GUILayout.Height(30)))
            {
                AddAssetsToLibrary();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        private void AddAssetsToLibrary()
        {
            // Validate inputs
            if (string.IsNullOrEmpty(_libraryPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a library file.", "OK");
                return;
            }

            if (!LibraryUtilities.IsValidLibraryFile(_libraryPath))
            {
                EditorUtility.DisplayDialog("Error", "Invalid library file selected.", "OK");
                return;
            }

            // Build asset add requests
            var requests = new List<LibraryWriter.AssetAddRequest>();

            foreach (var asset in _selectedAssets)
            {
                var request = new LibraryWriter.AssetAddRequest
                {
                    SourcePath = asset.FullPath,
                    AssetName = asset.Name,
                    AssetType = asset.Type,
                    Group = _group,
                    Tags = ParseTags(_tags),
                    Description = _description
                };

                if (_customThumbnail != null)
                {
                    var thumbPath = AssetDatabase.GetAssetPath(_customThumbnail);
                    if (!string.IsNullOrEmpty(thumbPath))
                    {
                        request.CustomThumbnailPath = System.IO.Path.Combine(
                            System.IO.Path.GetDirectoryName(Application.dataPath),
                            thumbPath
                        );
                    }
                }

                requests.Add(request);
            }

            // Show progress
            EditorUtility.DisplayProgressBar("Adding Assets", "Adding assets to library...", 0.5f);

            try
            {
                // Add assets to library
                bool success = LibraryWriter.AddAssetsToLibrary(_libraryPath, requests);

                EditorUtility.ClearProgressBar();

                if (success)
                {
                    // Save library path for next time
                    EditorPrefs.SetString(LastLibraryPathKey, _libraryPath);

                    EditorUtility.DisplayDialog("Success", $"Successfully added {_selectedAssets.Count} asset(s) to library!", "OK");
                    Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to add assets to library. Check the console for details.", "OK");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", $"Unexpected error: {ex.Message}", "OK");
                LibraryUtilities.LogError(ex.Message);
            }
        }

        private List<string> ParseTags(string tagsString)
        {
            var tags = new List<string>();

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
