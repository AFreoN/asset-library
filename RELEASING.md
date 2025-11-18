# Releasing Cross Project Asset Library

This document describes the process for building and releasing new versions of the Cross Project Asset Library package.

## Version Numbering

The project uses [Semantic Versioning](https://semver.org/) (MAJOR.MINOR.PATCH):

- **MAJOR**: Breaking changes or major features
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes and small improvements

Current version: `0.1.0`

## Release Checklist

### 1. Prepare Changes
- [ ] Ensure all features are completed and tested
- [ ] All code is committed to the `main` branch
- [ ] Run manual testing in Unity (compile, functionality, UI)

### 2. Update Version Number

Update the version in `package.json`:

```json
{
  "version": "X.Y.Z"
}
```

### 3. Update CHANGELOG.md

Add a new section at the top following [Keep a Changelog](https://keepachangelog.com/) format:

```markdown
## [X.Y.Z] - YYYY-MM-DD

### Added
- Feature 1
- Feature 2

### Changed
- Modification 1

### Fixed
- Bug fix 1

### Removed
- Deprecated feature
```

Example:
```markdown
## [0.2.0] - 2025-01-15

### Added
- Cloud storage integration
- Asset versioning support

### Fixed
- Fixed library loading issue on macOS

### Changed
- Improved search performance
```

### 4. Commit Changes

```bash
git add package.json CHANGELOG.md
git commit -m "Release v0.2.0"
```

### 5. Create Git Tag

```bash
git tag -a v0.2.0 -m "Release version 0.2.0"
```

The tag format **MUST** be `v` followed by the semantic version (e.g., `v0.1.0`, `v0.2.0`).

### 6. Push to GitHub

```bash
git push origin main
git push origin v0.2.0
```

The GitHub Actions workflow will automatically:
- Detect the new tag
- Extract the changelog for this version
- Create a GitHub Release with the changelog as the release notes

## Installation Methods After Release

### 1. Via Git URL (Recommended for Development)

In your Unity project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.crossproject.assetlibrary": "https://github.com/AFreoN/asset-library.git#v0.2.0"
  }
}
```

Or latest version:
```json
{
  "dependencies": {
    "com.crossproject.assetlibrary": "https://github.com/AFreoN/asset-library.git"
  }
}
```

### 2. Download Release

Download the source as a ZIP from the [Releases page](https://github.com/AFreoN/asset-library/releases) and extract to `Assets/Packages/com.crossproject.assetlibrary/`.

### 3. Manual Installation

Clone the repository into your project:

```bash
git clone https://github.com/AFreoN/asset-library.git Assets/Packages/com.crossproject.assetlibrary
```

## Verifying a Release

1. Go to [GitHub Releases](https://github.com/AFreoN/asset-library/releases)
2. Verify the release appears with the correct version
3. Check that the changelog is populated correctly
4. Download and test the package in a test Unity project

## Troubleshooting

### Release Not Created

- Verify the tag was pushed: `git push origin v0.2.0`
- Check GitHub Actions workflow: Go to **Actions** tab in the repository
- Ensure tag format is correct: `vX.Y.Z`

### Changelog Not Populated

- Verify the section exists in `CHANGELOG.md` with exact format: `## [X.Y.Z] - YYYY-MM-DD`
- Check workflow logs for parsing errors

### Tag Already Exists

To retag (if needed):

```bash
git tag -d v0.2.0  # Delete local tag
git push origin :refs/tags/v0.2.0  # Delete remote tag
git tag -a v0.2.0 -m "Release version 0.2.0"
git push origin v0.2.0
```

## Publishing to Unity Asset Store (Future)

If you wish to publish to the Unity Asset Store in the future:

1. Register as a publisher
2. Prepare marketing materials (description, screenshots, icon)
3. Upload the package version
4. Submit for review

Requires a packaged `.unitypackage` file. Contact Unity for detailed requirements.

## Continuous Integration Notes

- The workflow file is located at `.github/workflows/release.yml`
- It uses `softprops/action-gh-release` for creating releases
- The workflow requires GitHub token (automatically provided by GitHub Actions)

## Contact & Support

For issues with the release process, please check:
- GitHub Issues: https://github.com/AFreoN/asset-library/issues
- CHANGELOG.md for version history
