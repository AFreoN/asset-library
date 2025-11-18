# Cross Project Asset Library

A Unity Editor extension that provides a personal cross-project asset library, allowing developers to quickly import frequently used assets without manual folder navigation.

**Package ID**: `com.crossproject.assetlibrary`

## Features

- **Add Assets**: Right-click any asset in Unity → "Add to Asset Library" with metadata (group, tags, description, custom thumbnails)
- **Browse Library**: Open library window to view all saved assets with previews
- **Search & Filter**: Find assets by name, type, tags, or group
- **Import Assets**: Click or drag-drop assets from library into current project
- **Edit Metadata**: Modify asset properties (tags, group, description) within library
- **Portable Format**: Libraries stored as `.unitylib` files (ZIP-based) for easy backup and sharing

## Installation

1. Clone or download the package into your project's `Assets/Packages/` folder
2. Restart Unity
3. Access the tool via `Window → Asset Library` menu

## Quick Start

### Creating a New Library

1. Go to `Window → Asset Library`
2. Click "Create New Library"
3. Enter a name and choose a save location
4. The library will be created as a `.unitylib` file

### Adding Assets to Your Library

1. In your Project window, right-click on any asset
2. Select "Add to Asset Library"
3. Fill in the metadata:
   - **Group/Category**: Organize assets logically (e.g., "UI Elements", "Player Systems")
   - **Tags**: Add comma-separated tags for easy filtering (e.g., "ui, pixel-art")
   - **Description**: Optional description for the asset
   - **Custom Thumbnail**: For non-image assets, optionally provide a preview image
4. Select the target library file
5. Click "Add to Library"

### Browsing and Importing Assets

1. Open `Window → Asset Library`
2. Click "Browse" and select a `.unitylib` file
3. Use the search bar to find assets by name
4. Filter by:
   - **Type**: Asset type (Texture, Prefab, Script, etc.)
   - **Tag**: Filter by tags
   - **Group**: Filter by category
5. Click on assets to select them (Ctrl+Click for multi-select)
6. Click "Import Selected" to import into your project

### Drag and Drop (Alternative Import)

The library supports drag-and-drop import. Simply drag assets from the library window to your Project window.

## Supported Asset Types

The tool automatically categorizes and handles:

- **Textures**: PNG, JPG, TGA, PSD, EXR, etc.
- **Prefabs**: `.prefab` files
- **Scripts**: C# MonoScripts
- **Materials**: `.mat` files
- **Shaders**: `.shader` files
- **Audio**: MP3, WAV, OGG, AIF files
- **Models**: FBX, OBJ, Blend files
- **Animations**: Animation clips and controllers
- **Other**: Any other Unity-compatible asset types

## Library File Format

Libraries are stored as `.unitylib` files (ZIP archives) with the following structure:

```
MyLibrary.unitylib
├── manifest.json          # Asset metadata and library info
├── assets/
│   ├── textures/          # Texture assets
│   ├── prefabs/           # Prefab assets
│   ├── scripts/           # Script assets
│   ├── materials/         # Material assets
│   └── ...other types
└── thumbnails/            # Custom thumbnail images
```

## Preferences

The tool stores the following preferences (accessible via `EditorPrefs`):

- `CPAM.LastLibraryPath`: Path to the last opened library
- `CPAM.DefaultImportPath`: Default folder for importing assets
- `CPAM.WindowPosition`: Window position and size

## Limitations (MVP Scope)

The current version does NOT support:

- ❌ Cloud sync or remote storage
- ❌ Team collaboration features
- ❌ Asset versioning or update detection
- ❌ Dependency tracking between assets
- ❌ Multiple library support (open one library at a time)
- ❌ Library encryption or protection
- ❌ Asset deletion UI (API available via `LibraryEditor.DeleteAssetFromLibrary()` for programmatic deletion)

## Technical Details

### Architecture

The package uses a modular architecture organized into four layers:

- **Data Layer**: `LibraryManifest`, `AssetMetadata` - Serializable data structures
- **Core Layer**: `UnityLibFileHandler`, `AssetLibraryLoader`, `LibraryWriter`, `AssetImporter`, `LibraryEditor` - Business logic and file operations
- **UI Layer**: `AssetLibraryWindow`, `AddAssetDialog`, `EditAssetMetadataDialog`, `AssetPropertiesDialog`, `AssetContextMenu`, `ContextMenuExtension`, `CreateNewLibraryDialog`, `DragAndDropHandler`, `AssetPreviewWindow` - Editor windows and dialogs
- **Utilities**: `LibraryUtilities` - Helper functions and common operations

### Building and Testing

The package includes an assembly definition (`com.crossproject.assetlibrary.asmdef`) that compiles the Editor scripts separately for better compilation times.

## Troubleshooting

### Library Won't Load

- Ensure the file has a `.unitylib` extension
- Check the Unity console for detailed error messages
- Try creating a new test library to verify the format

### Assets Won't Import

- Check that the import destination folder exists and is writable
- Verify the asset files exist in the library
- Review the Unity console for specific error messages

### Missing Thumbnails

- Image assets automatically use themselves as thumbnails
- For other asset types, provide a custom thumbnail when adding to the library
- If missing, a placeholder will be used

## Contributing

This is an open-source project. Feel free to:

- Report bugs and feature requests
- Submit improvements and bug fixes
- Share example libraries with the community

## License

MIT License - See LICENSE.md for details

## Support

For issues, questions, or suggestions, please refer to the project repository or contact the maintainers.

---

**Version**: 0.1.0
**Unity Version**: 2020.3 LTS and newer
**Platform Support**: Windows, macOS, Linux
