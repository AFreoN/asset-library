using UnityEditor;
using UnityEngine;

namespace CPAL
{
    /// <summary>
    /// Modal dialog displaying read-only asset properties.
    /// Shows all metadata for an asset without allowing edits.
    /// </summary>
    public class AssetPropertiesDialog : EditorWindow
    {
        private static AssetPropertiesDialog _instance;
        private AssetMetadata _asset;
        private Vector2 _scrollPosition = Vector2.zero;

        private const float WindowWidth = 400f;
        private const float WindowHeight = 500f;

        /// <summary>
        /// Show the properties dialog for an asset.
        /// </summary>
        public static void ShowDialog(AssetMetadata asset)
        {
            if (asset == null)
            {
                EditorUtility.DisplayDialog("Error", "Asset is null.", "OK");
                return;
            }

            // Close existing dialog if any
            if (_instance != null)
            {
                _instance.Close();
            }

            // Create new dialog
            _instance = CreateInstance<AssetPropertiesDialog>();
            _instance._asset = asset;
            _instance.minSize = new Vector2(WindowWidth, 300);
            _instance.maxSize = new Vector2(WindowWidth + 50, 800);
            _instance.titleContent = new GUIContent("Asset Properties");

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
            EditorGUILayout.LabelField("Asset Properties", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Basic Information
            EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.TextField("Name:", _asset.name);
            EditorGUILayout.TextField("Type:", _asset.type);
            EditorGUILayout.TextField("Asset ID:", _asset.id);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Organization
            EditorGUILayout.LabelField("Organization", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.TextField("Group/Category:", _asset.group ?? "");

            // Tags
            if (_asset.tags != null && _asset.tags.Count > 0)
            {
                EditorGUILayout.LabelField("Tags:", EditorStyles.label);
                EditorGUI.indentLevel++;
                foreach (var tag in _asset.tags)
                {
                    EditorGUILayout.TextField("", tag);
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("Tags:", "(None)");
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Description
            EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.TextArea(_asset.description ?? "", GUILayout.Height(60));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // File Information
            EditorGUILayout.LabelField("File Information", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.TextField("Relative Path:", _asset.relativePath);
            EditorGUILayout.TextField("File Size:", LibraryUtilities.FormatFileSize(_asset.fileSize));

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Dates
            EditorGUILayout.LabelField("Dates", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.TextField("Date Added:", _asset.dateAdded);

            EditorGUI.indentLevel--;

            GUILayout.FlexibleSpace();

            // Close Button
            EditorGUILayout.Space();
            if (GUILayout.Button("Close", GUILayout.Height(30)))
            {
                Close();
            }

            GUILayout.EndScrollView();
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
