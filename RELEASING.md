# Releasing Cross Project Asset Library

This document describes the complete process for building and releasing new versions of the Cross Project Asset Library (CPAL) package. It covers everything from preparation to verification and troubleshooting.

---

## Quick Start (TL;DR)

If you've released before, here's the quick reference:

```bash
# 1. Update version and changelog
# 2. Commit changes
git add package.json CHANGELOG.md
git commit -m "Release v1.0.0"

# 3. Create and push tag (this triggers automation)
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin main
git push origin v1.0.0

# Done! GitHub Actions creates the release automatically
```

---

## Version Numbering

The project uses [Semantic Versioning](https://semver.org/) (MAJOR.MINOR.PATCH):

- **MAJOR**: Breaking changes or major features
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes and small improvements

**Current Version**: 1.0.0
**Next Release**: Determine based on changes

---

## Complete Release Process

### Step 1: Prepare Changes

- [ ] Ensure all features are completed and tested
- [ ] All code is committed to the `main` branch
- [ ] Run manual testing in Unity (compilation, functionality, UI)
- [ ] Get code review approval if applicable

### Step 2: Update Version Number

Edit `package.json`:

```json
{
  "version": "X.Y.Z"
}
```

**Examples:**
- `1.0.0` → Major release (stability, significant features)
- `1.1.0` → New features (backward compatible)
- `1.0.1` → Bug fixes only

### Step 3: Update CHANGELOG.md

Add a new section at the **top** following [Keep a Changelog](https://keepachangelog.com/) format:

```markdown
## [X.Y.Z] - YYYY-MM-DD

### Added
- New feature 1
- New feature 2

### Changed
- Modified behavior 1
- Updated component 2

### Fixed
- Fixed bug 1
- Fixed bug 2

### Removed
- Removed deprecated feature
```

**Complete Example:**

```markdown
## [1.1.0] - 2025-12-01

### Added
- Recent libraries quick access (MRU tracking)
- File system monitoring for auto-reload
- Advanced asset preview window (textures, audio, 3D meshes)

### Changed
- Improved library validation and cleanup
- Enhanced import dialog with destination selection

### Fixed
- Fixed library loading on Windows paths with spaces
- Fixed thumbnail generation for custom assets

### Removed
- Removed legacy EditorPrefs migration code
```

### Step 4: Commit Changes

```bash
git add package.json CHANGELOG.md
git commit -m "Release v1.1.0"
git push origin main
```

### Step 5: Create and Push Git Tag

The tag **MUST** be in format `vX.Y.Z`:

```bash
git tag -a v1.1.0 -m "Release version 1.1.0"
git push origin v1.1.0
```

⚠️ **Tag Format Rules**:
- ✅ Correct: `v1.0.0`, `v1.1.0`, `v0.2.1`
- ❌ Wrong: `1.0.0` (missing 'v'), `version-1.0`, `v1.0` (incomplete)

### Step 6: Verify Release (Automatic)

The GitHub Actions workflow automatically:
1. Detects the new tag pushed
2. Parses the CHANGELOG.md section
3. Creates a GitHub Release with the changelog as description
4. Makes it available for download

**No manual GitHub Release creation needed!**

---

## Installation Methods for Users

After you create a release, users can install via:

### Option 1: Git URL (Recommended for Development)

In their Unity project's `Packages/manifest.json`:

**Specific version:**
```json
{
  "dependencies": {
    "com.crossproject.assetlibrary": "https://github.com/AFreoN/asset-library.git#v1.0.0"
  }
}
```

**Latest version:**
```json
{
  "dependencies": {
    "com.crossproject.assetlibrary": "https://github.com/AFreoN/asset-library.git"
  }
}
```

### Option 2: Download Release

1. Go to [GitHub Releases](https://github.com/AFreoN/asset-library/releases)
2. Download the source code ZIP
3. Extract to `Assets/Packages/com.crossproject.assetlibrary/`

### Option 3: Clone Repository

```bash
git clone https://github.com/AFreoN/asset-library.git Assets/Packages/com.crossproject.assetlibrary
```

---

## Verifying Your Release

1. Go to [GitHub Releases](https://github.com/AFreoN/asset-library/releases)
2. Verify the release appears with the correct version number
3. Check that the changelog is populated correctly (should match CHANGELOG.md)
4. Download and test the package in a test Unity project
5. Confirm asset import/export functions work correctly

---

## Continuous Integration

**Workflow File**: `.github/workflows/release.yml`

The workflow:
- Triggers automatically when you push a tag matching `v*.*.*`
- Uses `softprops/action-gh-release` for creating releases
- Parses CHANGELOG.md for release notes
- Requires GitHub token (automatically provided by GitHub Actions)

---

## Troubleshooting

### Release Didn't Appear

**Problem**: GitHub Release wasn't created after pushing the tag.

**Solution**:
```bash
# Verify the tag was pushed
git tag -l
git push origin --tags

# Check GitHub Actions for errors
# Go to Actions tab in repository → Select latest workflow run
```

### Changelog Not Populated in Release

**Problem**: Release exists but changelog section is empty.

**Solution**:
- Verify the section in `CHANGELOG.md` matches exactly: `## [X.Y.Z] - YYYY-MM-DD`
- Check workflow logs for parsing errors
- Ensure no typos in version number

### Tag Already Exists

**Problem**: Trying to create a tag that already exists.

**Solution**:
```bash
# Delete local tag
git tag -d v1.0.0

# Delete remote tag
git push origin :refs/tags/v1.0.0

# Create the tag again
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

### Need to Fix an Existing Release

**Problem**: You made a mistake in the release (wrong version, incomplete changelog).

**Solution**:
1. Delete the tag (see above)
2. Fix the issue (update package.json, CHANGELOG.md)
3. Commit the fixes
4. Create the tag again

---

## Quick Git Commands Reference

```bash
# View local tags
git tag -l

# View remote tags
git tag -l -r

# Create annotated tag
git tag -a v1.0.0 -m "Release version 1.0.0"

# Push single tag
git push origin v1.0.0

# Push all tags
git push origin --tags

# Delete local tag
git tag -d v1.0.0

# Delete remote tag
git push origin :refs/tags/v1.0.0

# View git log with tags
git log --oneline --decorate
```

---

## Future: Unity Asset Store Publishing

If you wish to publish to the Unity Asset Store:

1. Register as a publisher
2. Prepare marketing materials (description, screenshots, icon)
3. Upload the package version as `.unitypackage`
4. Submit for review

Contact Unity for detailed requirements.

---

## Key Files

- **`package.json`** - Version number (update before release)
- **`CHANGELOG.md`** - Release notes (update before release)
- **`.github/workflows/release.yml`** - GitHub Actions automation (no manual editing needed)

---

## Support & Issues

For questions or issues with the release process:
- Check GitHub Issues: https://github.com/AFreoN/asset-library/issues
- Review CHANGELOG.md for version history
- Consult this document for detailed guidance
