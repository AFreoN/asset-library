using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CPAM
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

        private GUIStyle _listItemStyle;
        private GUIStyle _selectedItemStyle;

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

            EditorGUILayout.LabelField("Click to select, double-click to open:", EditorStyles.miniLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < _libraries.Count; i++)
            {
                DrawLibraryItem(_libraries[i], i);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draw a single library item in the list.
        /// </summary>
        private void DrawLibraryItem(RecentLibrariesManager.LibraryMetadata metadata, int index)
        {
            Rect itemRect = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));

            // Selection highlight
            bool isSelected = (_selectedLibrary != null && _selectedLibrary.path == metadata.path);
            if (isSelected)
            {
                GUI.DrawTexture(itemRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.5f, 0.8f) : new Color(0.3f, 0.6f, 0.9f);
                GUI.DrawTexture(itemRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = Color.white;
            }

            // Handle click events
            if (itemRect.Contains(Event.current.mousePosition))
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

            // Draw content
            EditorGUI.indentLevel++;

            Rect contentRect = new Rect(itemRect.x + 10, itemRect.y + 5, itemRect.width - 120, itemRect.height - 10);

            // Library name
            EditorGUI.LabelField(
                new Rect(contentRect.x, contentRect.y, contentRect.width, 18),
                metadata.libraryName,
                EditorStyles.boldLabel
            );

            // Asset count
            EditorGUI.LabelField(
                new Rect(contentRect.x, contentRect.y + 18, contentRect.width, 14),
                $"Assets: {metadata.assetCount}",
                EditorStyles.miniLabel
            );

            // Last accessed
            EditorGUI.LabelField(
                new Rect(contentRect.x, contentRect.y + 32, contentRect.width, 14),
                $"Accessed: {FormatDateTime(metadata.lastAccessed)}",
                EditorStyles.miniLabel
            );

            // File path
            EditorGUI.LabelField(
                new Rect(contentRect.x, contentRect.y + 46, contentRect.width, 12),
                metadata.path,
                EditorStyles.miniLabel
            );

            EditorGUI.indentLevel--;

            // Right-side buttons
            Rect buttonRect = new Rect(itemRect.x + itemRect.width - 110, itemRect.y + 15, 100, 20);

            if (GUI.Button(buttonRect, "Remove"))
            {
                RecentLibrariesManager.Instance.RemoveLibrary(metadata.path);
                RefreshLibraryList();
                Event.current.Use();
                return;
            }
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
            if (_listItemStyle == null)
            {
                _listItemStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(10, 10, 5, 5),
                    margin = new RectOffset(0, 0, 2, 2)
                };

                _selectedItemStyle = new GUIStyle(_listItemStyle)
                {
                    normal = { background = EditorGUIUtility.whiteTexture }
                };
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
