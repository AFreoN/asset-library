# Installation Guide

## Prerequisites

- Unity 2020.3 LTS or newer
- Git (for git-based installation methods)

## Installation Methods

### Method 1: Git URL (Recommended)

Add the package to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.crossproject.assetlibrary": "https://github.com/AFreoN/asset-library.git#v0.1.0"
  }
}
```

**For the latest version (no specific release):**
```json
{
  "dependencies": {
    "com.crossproject.assetlibrary": "https://github.com/AFreoN/asset-library.git"
  }
}
```

Then restart Unity. The package manager will automatically download and install the package.

### Method 2: Download Release

1. Go to [GitHub Releases](https://github.com/AFreoN/asset-library/releases)
2. Download the source code (ZIP) for your desired version
3. Extract the contents
4. Copy the package folder to `Assets/Packages/com.crossproject.assetlibrary/`
5. Restart Unity

### Method 3: Clone Repository

```bash
# Inside your Unity project directory
mkdir -p Assets/Packages
cd Assets/Packages
git clone https://github.com/AFreoN/asset-library.git com.crossproject.assetlibrary
```

Then restart Unity.

## Verification

After installation:

1. Open Unity
2. Go to **Window → Asset Library** menu
3. You should see the Asset Library window appear
4. Try creating a test library to verify functionality

## Updating the Package

### If using Method 1 (Git URL)

Manually update the version in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.crossproject.assetlibrary": "https://github.com/AFreoN/asset-library.git#v0.2.0"
  }
}
```

Then restart Unity.

### If using Method 3 (Cloned Repository)

Pull the latest changes:

```bash
cd Assets/Packages/com.crossproject.assetlibrary
git pull origin main
```

Or checkout a specific version:

```bash
git checkout v0.2.0
```

## Troubleshooting

### Package Not Appearing in Window Menu

- Verify the package folder structure is correct
- Check the Assembly Definition file exists: `Editor/com.crossproject.assetlibrary.asmdef`
- Try reimporting assets: right-click the package folder → **Reimport**

### Compilation Errors

- Ensure your Unity version is 2020.3 LTS or newer
- Check the console for specific error messages
- Try deleting the `Library` folder in your project and reopening Unity

### Git Installation Issues

- Ensure Git is installed and in your system PATH
- For HTTPS URLs, you may need to configure credentials
- For HTTPS authentication on Windows, consider using Git Credential Manager

### Missing Assets or Metadata

- Clear the library cache by deleting `%TEMP%` folders (search for `CPAL_` prefixed folders)
- Close and reopen the Asset Library window
- Verify the `.unitylib` file is not corrupted

## Next Steps

After installation, see [README.md](README.md) for usage instructions.
