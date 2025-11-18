using System.IO;
using UnityEditor;
using UnityEngine;

namespace CPAL
{
    /// <summary>
    /// EditorWindow for previewing assets from the library.
    /// Displays asset-specific previews using Unity's built-in preview system.
    /// </summary>
    public class AssetPreviewWindow : EditorWindow
    {
        private static AssetPreviewWindow _instance;
        private AssetMetadata _asset;
        private AssetLibraryLoader _loader;
        private Vector2 _scrollPosition = Vector2.zero;
        private Texture2D _previewTexture;
        private PreviewRenderUtility _previewRenderer;
        private Vector3 _meshRotation = Vector3.zero;
        private float _meshZoom = 1f;

        // Audio preview state
        private AudioClip _audioClip;

        // Text preview state
        private string _cachedTextContent;
        private string _cachedTextPath;
        private Vector2 _textScrollPosition = Vector2.zero;
        private const int MaxTextPreviewChars = 50000; // Limit for performance

        private const float PreviewSize = 300f;
        private const float WindowWidth = 900f;
        private const float WindowHeight = 650f;

        /// <summary>
        /// Show the asset preview window.
        /// </summary>
        public static void ShowPreview(AssetMetadata asset, AssetLibraryLoader loader)
        {
            if (asset == null || loader == null)
            {
                EditorUtility.DisplayDialog("Error", "Invalid asset or loader.", "OK");
                return;
            }

            // Close existing window if any
            if (_instance != null)
            {
                _instance.Close();
            }

            // Create new window
            _instance = CreateInstance<AssetPreviewWindow>();
            _instance._asset = asset;
            _instance._loader = loader;
            _instance.titleContent = new GUIContent($"Preview - {asset.name}");

            // Center the window
            var rect = EditorGUIUtility.GetMainWindowPosition();
            var x = (rect.width - WindowWidth) / 2 + rect.x;
            var y = (rect.height - WindowHeight) / 2 + rect.y;
            _instance.position = new Rect(x, y, WindowWidth, WindowHeight);

            _instance.Show();
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
            EditorGUILayout.LabelField($"Preview: {_asset.name}", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Layout: Preview on left, metadata on right
            EditorGUILayout.BeginHorizontal();

            // Left panel: Preview (type-specific)
            EditorGUILayout.BeginVertical(GUILayout.Width(PreviewSize + 20));
            DrawAssetPreview();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(20);

            // Right panel: Metadata
            EditorGUILayout.BeginVertical();
            DrawMetadataPanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        private void DrawAssetPreview()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            var previewRect = EditorGUILayout.GetControlRect(GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
            GUI.Box(previewRect, "", GUI.skin.box);

            // Draw type-specific preview
            if (IsMeshAsset(_asset.type))
            {
                DrawMeshPreview(previewRect);
            }
            else if (IsAudioAsset(_asset.type))
            {
                DrawAudioPreview(previewRect);
            }
            else if (IsMaterialAsset(_asset.type))
            {
                DrawMaterialPreview(previewRect);
            }
            else if (IsTextureAsset(_asset.type))
            {
                DrawTexturePreview(previewRect);
            }
            else if (IsShaderAsset(_asset.type))
            {
                DrawShaderPreview(previewRect);
            }
            else if (IsTextAsset(_asset.type))
            {
                DrawTextPreview(previewRect);
            }
            else
            {
                GUI.Label(previewRect, $"No preview available\n({_asset.type})", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space();

            // File size info
            EditorGUILayout.TextField("File Size:", LibraryUtilities.FormatFileSize(_asset.fileSize));
        }

        private Mesh _cachedMesh;
        private string _cachedMeshPath;
        private Editor _meshEditor;

        private void DrawMeshPreview(Rect previewRect)
        {
            // Load mesh from asset if not cached
            if (_cachedMesh == null || _cachedMeshPath != _asset.relativePath)
            {
                LoadMesh();
            }

            if (_cachedMesh == null)
            {
                GUI.Label(previewRect, "Could not load mesh", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Failed to load mesh file", MessageType.Warning);
                return;
            }

            try
            {
                // Create editor for mesh if not already created
                if (_meshEditor == null)
                {
                    _meshEditor = Editor.CreateEditor(_cachedMesh);
                }

                // Use Unity's built-in mesh preview
                if (_meshEditor != null)
                {
                    _meshEditor.OnPreviewGUI(previewRect, EditorStyles.whiteLabel);
                }
                else
                {
                    GUI.Label(previewRect, "Could not create mesh preview", EditorStyles.centeredGreyMiniLabel);
                }
            }
            catch (System.Exception ex)
            {
                LibraryUtilities.LogWarning($"Failed to render mesh preview: {ex.Message}");
                GUI.Label(previewRect, "Could not render mesh", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("3D Mesh Preview (Unity Inspector)", MessageType.Info);
        }

        private void LoadMesh()
        {
            _cachedMesh = null;
            _cachedMeshPath = null;

            try
            {
                var meshData = _loader.GetAssetFile(_asset);
                if (meshData == null || meshData.Length == 0)
                {
                    LibraryUtilities.LogWarning("Could not load mesh data");
                    return;
                }

                // Extract mesh to temp Assets folder for loading
                string tempAssetsPath = "Assets/CPAL_TempMeshes";
                if (!System.IO.Directory.Exists(tempAssetsPath))
                {
                    System.IO.Directory.CreateDirectory(tempAssetsPath);
                    AssetDatabase.Refresh();
                }

                // Use original file extension to preserve format (FBX, OBJ, etc)
                var extension = System.IO.Path.GetExtension(_asset.relativePath);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".fbx"; // Default to FBX if no extension
                }

                string tempMeshPath = System.IO.Path.Combine(tempAssetsPath, _asset.name + extension);
                System.IO.File.WriteAllBytes(tempMeshPath, meshData);

                // Import and load - let Unity's importer handle the format
                AssetDatabase.ImportAsset(tempMeshPath, ImportAssetOptions.ForceUpdate);

                // Get the mesh from the imported asset
                // For FBX/OBJ files, we need to load the mesh from the model
                var importedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(tempMeshPath);
                if (importedAsset != null && importedAsset.GetComponent<MeshFilter>() != null)
                {
                    _cachedMesh = importedAsset.GetComponent<MeshFilter>().sharedMesh;
                }
                else
                {
                    // Try loading as a mesh directly (for .mesh files)
                    _cachedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(tempMeshPath);
                }

                _cachedMeshPath = _asset.relativePath;

                if (_cachedMesh != null)
                {
                    LibraryUtilities.Log($"Loaded mesh: {_asset.name}");
                }
                else
                {
                    LibraryUtilities.LogWarning("Failed to load mesh - could not extract from imported asset");
                }
            }
            catch (System.Exception ex)
            {
                LibraryUtilities.LogError($"Failed to load mesh: {ex.Message}");
            }
        }

        private Editor _audioEditor;

        private void DrawAudioPreview(Rect previewRect)
        {
            // Load audio clip from asset
            if (_audioClip == null)
            {
                LoadAudioClip();
            }

            if (_audioClip == null)
            {
                GUI.Label(previewRect, "Could not load audio clip", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Failed to load audio file", MessageType.Warning);
                return;
            }

            try
            {
                // Create editor for audio clip if not already created
                if (_audioEditor == null)
                {
                    _audioEditor = Editor.CreateEditor(_audioClip);
                }

                // Use Unity's built-in audio preview with player controls
                if (_audioEditor != null)
                {
                    _audioEditor.OnPreviewGUI(previewRect, EditorStyles.whiteLabel);
                }
                else
                {
                    GUI.Label(previewRect, "Could not create audio preview", EditorStyles.centeredGreyMiniLabel);
                }
            }
            catch (System.Exception ex)
            {
                LibraryUtilities.LogWarning($"Failed to render audio preview: {ex.Message}");
                GUI.Label(previewRect, "Could not render audio", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Audio Preview (Unity Inspector)", MessageType.Info);

            // Export button
            if (GUILayout.Button("📂 Export Audio", GUILayout.Height(30)))
            {
                ExportAudio();
            }
        }

        private void LoadAudioClip()
        {
            if (_audioClip != null)
                return;

            try
            {
                var audioData = _loader.GetAssetFile(_asset);
                if (audioData == null || audioData.Length == 0)
                {
                    LibraryUtilities.LogWarning("Could not load audio clip data");
                    return;
                }

                // Extract audio to temp Assets folder for loading
                string tempAssetsPath = "Assets/CPAL_TempAudio";
                if (!System.IO.Directory.Exists(tempAssetsPath))
                {
                    System.IO.Directory.CreateDirectory(tempAssetsPath);
                    AssetDatabase.Refresh();
                }

                // Get file extension from asset path
                var extension = Path.GetExtension(_asset.relativePath);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".wav";
                }

                var tempAudioPath = System.IO.Path.Combine(tempAssetsPath, _asset.name + extension);
                System.IO.File.WriteAllBytes(tempAudioPath, audioData);

                // Import and load
                AssetDatabase.ImportAsset(tempAudioPath);
                _audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(tempAudioPath);

                if (_audioClip != null)
                {
                    LibraryUtilities.Log($"Loaded audio clip: {_asset.name}");
                }
                else
                {
                    LibraryUtilities.LogWarning("Failed to load audio clip - AssetDatabase could not deserialize it");
                }
            }
            catch (System.Exception ex)
            {
                LibraryUtilities.LogError($"Failed to load audio clip: {ex.Message}");
            }
        }

        private void ExportAudio()
        {
            try
            {
                var audioData = _loader.GetAssetFile(_asset);
                if (audioData == null || audioData.Length == 0)
                {
                    EditorUtility.DisplayDialog("Error", "Could not load audio file", "OK");
                    return;
                }

                // Determine file extension
                var extension = Path.GetExtension(_asset.relativePath);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".ogg";
                }

                // Show save dialog
                var savePath = EditorUtility.SaveFilePanel(
                    "Export Audio",
                    "",
                    _asset.name,
                    extension.TrimStart('.')
                );

                if (string.IsNullOrEmpty(savePath))
                {
                    return; // User cancelled
                }

                // Write file
                File.WriteAllBytes(savePath, audioData);
                EditorUtility.DisplayDialog("Success", $"Audio exported to:\n{savePath}", "OK");
                LibraryUtilities.Log($"Exported audio: {_asset.name} to {savePath}");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to export audio: {ex.Message}", "OK");
                LibraryUtilities.LogError(ex.Message);
            }
        }

        private Material _cachedMaterial;
        private string _cachedMaterialPath;
        private Editor _materialEditor;

        private void DrawMaterialPreview(Rect previewRect)
        {
            // Load material from asset if not cached
            if (_cachedMaterial == null || _cachedMaterialPath != _asset.relativePath)
            {
                LoadMaterial();
            }

            if (_cachedMaterial == null)
            {
                GUI.Label(previewRect, "Could not load material", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Failed to load material file", MessageType.Warning);
                return;
            }

            try
            {
                // Create editor for material if not already created
                if (_materialEditor == null)
                {
                    _materialEditor = Editor.CreateEditor(_cachedMaterial);
                }

                // Use Unity's built-in material preview
                if (_materialEditor != null)
                {
                    _materialEditor.OnPreviewGUI(previewRect, EditorStyles.whiteLabel);
                }
                else
                {
                    GUI.Label(previewRect, "Could not create material preview", EditorStyles.centeredGreyMiniLabel);
                }
            }
            catch (System.Exception ex)
            {
                LibraryUtilities.LogWarning($"Failed to render material preview: {ex.Message}");
                GUI.Label(previewRect, "Could not render material", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Material Preview (Unity Inspector)", MessageType.Info);
        }

        private void LoadMaterial()
        {
            _cachedMaterial = null;
            _cachedMaterialPath = null;

            try
            {
                var materialData = _loader.GetAssetFile(_asset);
                if (materialData == null || materialData.Length == 0)
                {
                    LibraryUtilities.LogWarning("Could not load material data");
                    return;
                }

                // Extract material to temp Assets folder for loading
                string tempAssetsPath = "Assets/CPAL_TempMaterials";
                if (!System.IO.Directory.Exists(tempAssetsPath))
                {
                    System.IO.Directory.CreateDirectory(tempAssetsPath);
                    AssetDatabase.Refresh();
                }

                string tempMatPath = System.IO.Path.Combine(tempAssetsPath, _asset.name + ".mat");
                System.IO.File.WriteAllBytes(tempMatPath, materialData);

                // Import and load
                AssetDatabase.ImportAsset(tempMatPath);
                _cachedMaterial = AssetDatabase.LoadAssetAtPath<Material>(tempMatPath);
                _cachedMaterialPath = _asset.relativePath;

                if (_cachedMaterial != null)
                {
                    LibraryUtilities.Log($"Loaded material: {_asset.name}");
                }
                else
                {
                    LibraryUtilities.LogWarning("Failed to load material - AssetDatabase could not deserialize it");
                }
            }
            catch (System.Exception ex)
            {
                LibraryUtilities.LogError($"Failed to load material: {ex.Message}");
            }
        }

        private void DrawTexturePreview(Rect previewRect)
        {
            // Load texture from asset
            var textureData = _loader.GetAssetFile(_asset);
            if (textureData == null || textureData.Length == 0)
            {
                GUI.Label(previewRect, "Could not load texture data", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            try
            {
                var tempTexture = new Texture2D(1, 1);
                tempTexture.LoadImage(textureData);

                DrawTextureWithAspectRatio(previewRect, tempTexture);
                _previewTexture = tempTexture;
            }
            catch (System.Exception ex)
            {
                LibraryUtilities.LogWarning($"Failed to load texture: {ex.Message}");
                GUI.Label(previewRect, "Could not load texture", EditorStyles.centeredGreyMiniLabel);
            }
        }

        private void DrawShaderPreview(Rect previewRect)
        {
            EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.2f, 1f));
            GUI.Label(previewRect, "Shader Preview\n(code preview not available)", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Shader assets cannot be visually previewed.\nOpen the file in the editor to view the code.", MessageType.Info);
        }

        private void DrawTextPreview(Rect previewRect)
        {
            // Load text content if not cached
            if (_cachedTextContent == null || _cachedTextPath != _asset.relativePath)
            {
                LoadTextContent();
            }

            if (string.IsNullOrEmpty(_cachedTextContent))
            {
                GUI.Label(previewRect, "Could not load text file", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Failed to load text file", MessageType.Warning);
                return;
            }

            try
            {
                // Draw dark background
                EditorGUI.DrawRect(previewRect, new Color(0.15f, 0.15f, 0.15f, 1f));

                // Display text in a scrollable text area
                var textStyle = new GUIStyle(EditorStyles.textArea)
                {
                    richText = false,
                    wordWrap = true,
                    fontSize = 10,
                    padding = new RectOffset(4, 4, 4, 4)
                };

                // Calculate content dimensions
                float contentWidth = previewRect.width - 20; // Account for scrollbar and padding
                var textContent = new GUIContent(_cachedTextContent);
                float contentHeight = textStyle.CalcHeight(textContent, contentWidth);

                // Create virtual rect for scroll content
                var virtualRect = new Rect(0, 0, contentWidth, contentHeight);

                // Begin scroll view with proper content height
                _textScrollPosition = GUI.BeginScrollView(previewRect, _textScrollPosition, virtualRect);

                // Draw the text at origin within scroll view
                GUI.TextArea(new Rect(0, 0, contentWidth, contentHeight), _cachedTextContent, textStyle);

                GUI.EndScrollView();
            }
            catch (System.Exception ex)
            {
                LibraryUtilities.LogWarning($"Failed to render text preview: {ex.Message}");
                GUI.Label(previewRect, "Could not render text", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Text File Preview", MessageType.Info);
        }

        private void LoadTextContent()
        {
            _cachedTextContent = null;
            _cachedTextPath = null;

            try
            {
                var textData = _loader.GetAssetFile(_asset);
                if (textData == null || textData.Length == 0)
                {
                    LibraryUtilities.LogWarning("Could not load text file data");
                    return;
                }

                // Convert bytes to string using UTF-8 encoding
                _cachedTextContent = System.Text.Encoding.UTF8.GetString(textData);

                // Limit content size for performance
                if (_cachedTextContent.Length > MaxTextPreviewChars)
                {
                    _cachedTextContent = _cachedTextContent.Substring(0, MaxTextPreviewChars) +
                        $"\n\n... (truncated - file too large, showing {MaxTextPreviewChars} chars)";
                }

                _cachedTextPath = _asset.relativePath;
                LibraryUtilities.Log($"Loaded text file: {_asset.name}");
            }
            catch (System.Exception ex)
            {
                LibraryUtilities.LogError($"Failed to load text file: {ex.Message}");
            }
        }

        private void DrawTextureWithAspectRatio(Rect rect, Texture2D texture)
        {
            if (texture == null)
                return;

            float textureAspect = (float)texture.width / texture.height;
            float scaledHeight = rect.height;
            float scaledWidth = scaledHeight * textureAspect;

            if (scaledWidth > rect.width)
            {
                scaledWidth = rect.width;
                scaledHeight = scaledWidth / textureAspect;
            }

            float offsetX = (rect.width - scaledWidth) / 2;
            float offsetY = (rect.height - scaledHeight) / 2;

            var drawRect = new Rect(
                rect.x + offsetX,
                rect.y + offsetY,
                scaledWidth,
                scaledHeight
            );

            GUI.DrawTexture(drawRect, texture, ScaleMode.StretchToFill);
        }

        private void DrawMetadataPanel()
        {
            EditorGUILayout.LabelField("Asset Information", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.TextField("Name:", _asset.name);
            EditorGUILayout.TextField("Type:", _asset.type);
            EditorGUILayout.TextField("Asset ID:", _asset.id);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Organization", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.TextField("Group:", _asset.group ?? "(None)");

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

            EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.TextArea(_asset.description ?? "(No description)", GUILayout.Height(60));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("File Information", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.TextField("Path:", _asset.relativePath);
            EditorGUILayout.TextField("Size:", LibraryUtilities.FormatFileSize(_asset.fileSize));

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Dates", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.TextField("Added:", _asset.dateAdded);

            EditorGUI.indentLevel--;

            GUILayout.FlexibleSpace();

            // Close button
            EditorGUILayout.Space();
            if (GUILayout.Button("Close", GUILayout.Height(30)))
            {
                Close();
            }
        }

        private bool IsMeshAsset(string type)
        {
            return type == "Mesh" || type == "Model" || type == "GameObject";
        }

        private bool IsAudioAsset(string type)
        {
            return type == "AudioClip";
        }

        private bool IsMaterialAsset(string type)
        {
            return type == "Material";
        }

        private bool IsTextureAsset(string type)
        {
            return type == "Texture2D" || type == "Sprite" || type == "RenderTexture";
        }

        private bool IsShaderAsset(string type)
        {
            return type == "Shader" || type == "ComputeShader";
        }

        private bool IsTextAsset(string type)
        {
            return type == "MonoScript" || type == "TextFile" || type == "Text" ||
                   type == "TextAsset" || type == "JSON" || type == "XML" || type == "YAML";
        }

        private void OnDestroy()
        {
            // Cleanup textures
            if (_previewTexture != null)
            {
                DestroyImmediate(_previewTexture);
            }

            if (_previewRenderer != null)
            {
                _previewRenderer.Cleanup();
            }

            // Clean up editors
            if (_meshEditor != null)
            {
                DestroyImmediate(_meshEditor);
            }

            if (_materialEditor != null)
            {
                DestroyImmediate(_materialEditor);
            }

            if (_audioEditor != null)
            {
                DestroyImmediate(_audioEditor);
            }

            // Clean up temporary folders
            try
            {
                string tempMeshPath = "Assets/CPAL_TempMeshes";
                if (System.IO.Directory.Exists(tempMeshPath))
                {
                    System.IO.Directory.Delete(tempMeshPath, true);
                    System.IO.File.Delete(tempMeshPath + ".meta");
                }

                string tempMaterialPath = "Assets/CPAL_TempMaterials";
                if (System.IO.Directory.Exists(tempMaterialPath))
                {
                    System.IO.Directory.Delete(tempMaterialPath, true);
                    System.IO.File.Delete(tempMaterialPath + ".meta");
                }

                string tempAudioPath = "Assets/CPAL_TempAudio";
                if (System.IO.Directory.Exists(tempAudioPath))
                {
                    System.IO.Directory.Delete(tempAudioPath, true);
                    System.IO.File.Delete(tempAudioPath + ".meta");
                }

                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                LibraryUtilities.LogWarning($"Failed to clean up temp assets: {ex.Message}");
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
