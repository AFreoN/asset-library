using UnityEditor;
using UnityEngine;

namespace CPAL
{
    /// <summary>
    /// Dialog for creating a new asset library.
    /// </summary>
    public class CreateNewLibraryDialog : EditorWindow
    {
        private static CreateNewLibraryDialog _instance;

        private string _libraryName = "My Asset Library";
        private string _libraryPath = "";

        private const float WindowWidth = 400f;
        private const float WindowHeight = 200f;

        /// <summary>
        /// Callback when a new library is created.
        /// </summary>
        public delegate void OnLibraryCreatedDelegate(string libraryPath);
        private static OnLibraryCreatedDelegate _onLibraryCreated;

        /// <summary>
        /// Show the create new library dialog.
        /// </summary>
        public static void ShowDialog(OnLibraryCreatedDelegate onLibraryCreated = null)
        {
            if (_instance != null)
            {
                _instance.Close();
            }

            _onLibraryCreated = onLibraryCreated;
            _instance = CreateInstance<CreateNewLibraryDialog>();
            _instance.minSize = new Vector2(WindowWidth, 150);
            _instance.maxSize = new Vector2(WindowWidth + 50, 300);
            _instance.titleContent = new GUIContent("Create New Library");

            var rect = EditorGUIUtility.GetMainWindowPosition();
            var x = (rect.width - WindowWidth) / 2 + rect.x;
            var y = (rect.height - WindowHeight) / 2 + rect.y;
            _instance.position = new Rect(x, y, WindowWidth, WindowHeight);

            // Use Show() instead of ShowModal() to allow SaveFilePanel to work properly
            // Modal windows have restrictions on nested dialogs like file panels
            _instance.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Create New Asset Library", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _libraryName = EditorGUILayout.TextField("Library Name:", _libraryName);

            EditorGUILayout.BeginHorizontal();
            _libraryPath = EditorGUILayout.TextField("Save Location:", _libraryPath);

            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                var path = EditorUtility.SaveFilePanel("Save Library As", "", "MyLibrary", "unitylib");
                if (!string.IsNullOrEmpty(path))
                {
                    _libraryPath = path;
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                Close();
            }

            if (GUILayout.Button("Create", GUILayout.Height(30)))
            {
                CreateLibrary();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CreateLibrary()
        {
            if (string.IsNullOrEmpty(_libraryName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a library name.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(_libraryPath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a save location.", "OK");
                return;
            }

            EditorUtility.DisplayProgressBar("Creating Library", "Creating new asset library...", 0.5f);

            try
            {
                if (LibraryWriter.CreateNewLibrary(_libraryPath, _libraryName))
                {
                    EditorUtility.ClearProgressBar();

                    // Store the path before deferring
                    string createdLibraryPath = _libraryPath;

                    // Defer window close and callback to next frame to prevent layout group conflicts
                    // This prevents issues when modal dialogs interact during the same GUI event processing cycle
                    EditorApplication.delayCall += () =>
                    {
                        // Close the dialog
                        Close();

                        // Invoke the callback to notify the caller (AssetLibraryWindow) to load the new library
                        _onLibraryCreated?.Invoke(createdLibraryPath);

                        // Show success message after dialog is closed and callback invoked
                        EditorUtility.DisplayDialog("Success", $"Library created at:\n{createdLibraryPath}", "OK");
                    };
                }
                else
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Error", "Failed to create library. Check the console for details.", "OK");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", $"Unexpected error: {ex.Message}", "OK");
                LibraryUtilities.LogError(ex.Message);
            }
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
