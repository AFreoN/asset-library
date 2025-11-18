using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CPAL
{
    /// <summary>
    /// Modal dialog showing recently used libraries.
    /// Allows user to select a library to load, or remove entries from the recent list.
    /// </summary>
    public class RecentLibrariesDialog : EditorWindow
    {
        private Vector2 _scrollPosition = Vector2.zero;
        private List<RecentLibrariesManager.LibraryMetadata> _libraries = new List<RecentLibrariesManager.LibraryMetadata>();
        private RecentLibrariesManager.LibraryMetadata _selectedLibrary = null;
        private bool _isLoadingData = false;

        private GUIStyle _headerStyle;
        private GUIStyle _rowStyle;
        private GUIStyle _selectedRowStyle;

        public delegate void OnLibrarySelectedDelegate(string libraryPath);
        private static OnLibrarySelectedDelegate _onLibrarySelected;

        /// <summary>
        /// Show the Recent Libraries dialog.
        /// </summary>
        public static void ShowDialog(OnLibrarySelectedDelegate onLibrarySelected)
        {
            _onLibrarySelected = onLibrarySelected;

            RecentLibrariesDialog window = GetWindow<RecentLibrariesDialog>("Recent Libraries", true);
            window.minSize = new Vector2(600, 400);
            window.maxSize = new Vector2(900, 700);
        }

        private void OnEnable()
        {
            RefreshLibraryList();
        }

        private void RefreshLibraryList()
        {
            _isLoadingData = true;
            _libraries = RecentLibrariesManager.Instance.GetRecentLibraries();
            _isLoadingData = false;
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawLibraryList();
            DrawFooter();
        }

        /// <summary>
        /// Draw header with title and info.
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Recent Libraries", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Total libraries: {_libraries.Count}", EditorStyles.miniLabel);
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Draw the scrollable library list.
        /// </summary>
        private void DrawLibraryList()
        {
            if (_isLoadingData)
            {
                EditorGUILayout.LabelField("Loading libraries...", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (_libraries.Count == 0)
            {
                EditorGUILayout.LabelField("No recent libraries. Open a library to add it to this list.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            InitializeStyles();

            // Draw table header
            DrawTableHeader();

            // Draw scrollable list
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < _libraries.Count; i++)
            {
                DrawLibraryItem(_libraries[i], i);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draw the table header with column labels.
        /// </summary>
        private void DrawTableHeader()
        {
            Rect headerRect = GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true));

            // Draw header background
            GUI.color = EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.9f, 0.9f, 0.9f);
            GUI.DrawTexture(headerRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = Color.white;

            // Draw header text
            EditorGUI.LabelField(new Rect(headerRect.x + 10, headerRect.y, 200, headerRect.height), "Name", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(headerRect.x + 220, headerRect.y, 80, headerRect.height), "Assets", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(headerRect.x + 310, headerRect.y, 120, headerRect.height), "Accessed", EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(headerRect.x + headerRect.width - 180, headerRect.y, 170, headerRect.height), "Actions", EditorStyles.boldLabel);

            // Draw separator
            GUI.color = EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            GUI.DrawTexture(new Rect(headerRect.x, headerRect.y + headerRect.height - 1, headerRect.width, 1), EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
        }

        /// <summary>
        /// Draw a single library item in the list.
        /// </summary>
        private void DrawLibraryItem(RecentLibrariesManager.LibraryMetadata metadata, int index)
        {
            Rect itemRect = GUILayoutUtility.GetRect(0, 32, GUILayout.ExpandWidth(true));

            // Selection highlight
            bool isSelected = (_selectedLibrary != null && _selectedLibrary.path == metadata.path);
            if (isSelected)
            {
                GUI.color = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.5f, 0.8f) : new Color(0.7f, 0.85f, 1f);
                GUI.DrawTexture(itemRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = Color.white;
            }

            // Alternate row colors for better readability
            if (index % 2 == 0)
            {
                GUI.color = EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.95f, 0.95f, 0.95f);
                GUI.DrawTexture(itemRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = Color.white;
            }

            // Right-side action buttons
            float buttonWidth = 75;
            float spacing = 5;
            float startX = itemRect.x + itemRect.width - (buttonWidth * 2 + spacing) - 10;

            // Define button rects first for click detection
            Rect revealButtonRect = new Rect(startX, itemRect.y + 6, buttonWidth, 20);
            Rect removeButtonRect = new Rect(startX + buttonWidth + spacing, itemRect.y + 6, buttonWidth, 20);

            // Handle click events on the row (but not on buttons)
            if (itemRect.Contains(Event.current.mousePosition))
            {
                // Check if click is NOT on any button
                if (!revealButtonRect.Contains(Event.current.mousePosition) &&
                    !removeButtonRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        _selectedLibrary = metadata;
                        Event.current.Use();

                        // Double-click to open
                        if (Event.current.clickCount == 2)
                        {
                            OpenSelectedLibrary();
                        }
                    }
                }
            }

            // Library name
            EditorGUI.LabelField(
                new Rect(itemRect.x + 10, itemRect.y + 8, 200, 16),
                metadata.libraryName,
                EditorStyles.label
            );

            // Asset count
            EditorGUI.LabelField(
                new Rect(itemRect.x + 220, itemRect.y + 8, 80, 16),
                $"{metadata.assetCount}",
                EditorStyles.miniLabel
            );

            // Last accessed
            EditorGUI.LabelField(
                new Rect(itemRect.x + 310, itemRect.y + 8, 120, 16),
                FormatDateTime(metadata.lastAccessed),
                EditorStyles.miniLabel
            );

            // Reveal button
            if (GUI.Button(revealButtonRect, "Reveal"))
            {
                RevealLibraryInExplorer(metadata.path);
                Event.current.Use();
            }

            // Remove button
            if (GUI.Button(removeButtonRect, "Remove"))
            {
                RecentLibrariesManager.Instance.RemoveLibrary(metadata.path);
                RefreshLibraryList();
                Event.current.Use();
                return;
            }

            // Draw bottom border
            GUI.color = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.8f, 0.8f, 0.8f);
            GUI.DrawTexture(new Rect(itemRect.x, itemRect.y + itemRect.height - 1, itemRect.width, 1), EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
        }

        /// <summary>
        /// Draw footer with action buttons.
        /// </summary>
        private void DrawFooter()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();

            if (_libraries.Count > 0 && GUILayout.Button("Clear All", GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Clear All Libraries",
                    "Are you sure you want to clear all recent libraries?",
                    "Yes", "No"))
                {
                    RecentLibrariesManager.Instance.ClearAll();
                    RefreshLibraryList();
                    _selectedLibrary = null;
                }
            }

            GUILayout.FlexibleSpace();

            if (_selectedLibrary == null)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Open Selected", GUILayout.Width(120)))
            {
                OpenSelectedLibrary();
            }

            GUI.enabled = true;

            if (GUILayout.Button("Close", GUILayout.Width(100)))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Open the selected library and close the dialog.
        /// </summary>
        private void OpenSelectedLibrary()
        {
            if (_selectedLibrary != null)
            {
                _onLibrarySelected?.Invoke(_selectedLibrary.path);
                Close();
            }
        }

        /// <summary>
        /// Initialize GUI styles.
        /// </summary>
        private void InitializeStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold
                };

                _rowStyle = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(10, 10, 0, 0)
                };

                _selectedRowStyle = new GUIStyle(_rowStyle)
                {
                    normal = { background = EditorGUIUtility.whiteTexture }
                };
            }
        }

        /// <summary>
        /// Reveal the library file in the system file explorer.
        /// </summary>
        private void RevealLibraryInExplorer(string libraryPath)
        {
            if (string.IsNullOrEmpty(libraryPath) || !File.Exists(libraryPath))
            {
                EditorUtility.DisplayDialog("Error", "Library file not found: " + libraryPath, "OK");
                return;
            }

            try
            {
                #if UNITY_EDITOR_WIN
                    // On Windows, use explorer /select with proper path escaping
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = "explorer.exe",
                        Arguments = "/select,\"" + libraryPath.Replace("/", "\\") + "\"",
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                #elif UNITY_EDITOR_OSX
                    // On macOS, use open -R
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = "open",
                        Arguments = "-R \"" + libraryPath + "\"",
                        UseShellExecute = false
                    };
                    System.Diagnostics.Process.Start(psi);
                #elif UNITY_EDITOR_LINUX
                    // For Linux, open the folder
                    string folder = System.IO.Path.GetDirectoryName(libraryPath);
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = "xdg-open",
                        Arguments = "\"" + folder + "\"",
                        UseShellExecute = false
                    };
                    System.Diagnostics.Process.Start(psi);
                #endif

                LibraryUtilities.Log("Revealing library: " + libraryPath);
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Error", "Failed to open file explorer: " + ex.Message, "OK");
                LibraryUtilities.LogError("Failed to reveal library in explorer: " + ex.Message);
            }
        }

        /// <summary>
        /// Format DateTime for display.
        /// </summary>
        private string FormatDateTime(string isoString)
        {
            if (string.IsNullOrEmpty(isoString))
            {
                return "Unknown";
            }

            try
            {
                System.DateTime dt = System.DateTime.Parse(isoString);
                System.DateTime now = System.DateTime.UtcNow;
                System.TimeSpan diff = now - dt;

                if (diff.TotalMinutes < 1)
                    return "Just now";
                if (diff.TotalHours < 1)
                    return $"{(int)diff.TotalMinutes}m ago";
                if (diff.TotalDays < 1)
                    return $"{(int)diff.TotalHours}h ago";
                if (diff.TotalDays < 7)
                    return $"{(int)diff.TotalDays}d ago";

                return dt.ToString("yyyy-MM-dd");
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
