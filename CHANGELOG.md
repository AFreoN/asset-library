# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2025-10-25

### Added

- Initial MVP release of Cross Project Asset Manager
- **Add Assets Feature**:
  - Right-click context menu for adding assets to library
  - Metadata input dialog (group, tags, description, custom thumbnails)
  - Support for all major asset types (textures, prefabs, scripts, materials, audio, models, animations, shaders)
  - Automatic asset categorization by type

- **Asset Library Management**:
  - Create new library files (.unitylib format)
  - ZIP-based portable library format
  - JSON manifest for asset metadata
  - Support for asset thumbnails (auto-generated for images, custom for others)

- **Asset Browser Window**:
  - Main editor window accessible via `Window → Asset Library`
  - Grid view with asset thumbnails
  - Asset name, type, and tags display
  - Multi-select support with Ctrl+Click

- **Search and Filtering**:
  - Text search by asset name (case-insensitive)
  - Filter by asset type (dropdown)
  - Filter by tag (dropdown)
  - Filter by group/category (dropdown)
  - Combined filtering support

- **Asset Import**:
  - Import button for selected assets
  - Automatic destination folder selection with dialog
  - Duplicate filename handling
  - Batch import support
  - AssetDatabase refresh integration

- **Asset Metadata Management**:
  - Edit asset metadata (tags, group, description) within library
  - Update asset properties without re-adding

- **Asset Deletion**:
  - Programmatic deletion API via `LibraryEditor.DeleteAssetFromLibrary()`
  - Handles asset file and thumbnail cleanup
  - Manifest updates on deletion
  - (Note: No UI for deletion in MVP)

- **Documentation**:
  - Comprehensive README with usage guide
  - MIT License
  - Quick start instructions
  - Troubleshooting section

### Known Limitations

- Single library file at a time (no simultaneous multiple library support)
- No GUID preservation (new GUID on import)
- Basic thumbnail support (static images only)
- Simple search (name only, case-insensitive)
- Manual library file selection each session
- No dependency resolution for complex prefabs
- No built-in asset deletion from library

### Technical Details

- **Language**: C#
- **Target Platform**: Unity Editor (Windows, macOS, Linux)
- **Minimum Unity Version**: 2020.3 LTS
- **Dependencies**: System.IO.Compression, JsonUtility (built-in)
- **Assembly Definition**: com.crossproject.assetlibrary.asmdef (Editor only)

---

## Future Roadmap (Not MVP)

Potential features for future releases:

- Cloud storage integration
- Team collaboration features
- Asset versioning and update detection
- Dependency tracking
- Multiple library support
- Asset deletion UI
- Advanced search with filters on multiple fields
- Asset preview for 3D models and prefabs
- Library encryption
- Asset usage analytics
- Bulk operations (export, delete, reorganize)
- Library merging and splitting

---
