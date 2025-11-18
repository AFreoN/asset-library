# Quick Start: Making a Release

## TL;DR Version

```bash
# 1. Update version in package.json
# 2. Update CHANGELOG.md
# 3. Commit
git add package.json CHANGELOG.md
git commit -m "Release v0.2.0"

# 4. Create tag (MUST start with 'v')
git tag -a v0.2.0 -m "Release version 0.2.0"

# 5. Push (this triggers GitHub Actions automatically)
git push origin main
git push origin v0.2.0

# Done! GitHub Actions creates the release automatically
```

## Detailed Steps

### Step 1: Update Version

Edit `package.json`:
```json
{
  "version": "0.2.0"
}
```

### Step 2: Update CHANGELOG

Add to top of `CHANGELOG.md`:
```markdown
## [0.2.0] - 2025-01-15

### Added
- Feature 1
- Feature 2

### Fixed
- Bug 1
```

### Step 3: Commit

```bash
git add package.json CHANGELOG.md
git commit -m "Release v0.2.0"
git push origin main
```

### Step 4: Tag and Release

```bash
git tag -a v0.2.0 -m "Release version 0.2.0"
git push origin v0.2.0
```

**That's it!** The workflow will automatically create a GitHub Release.

## Check Your Release

1. Go to: https://github.com/AFreoN/asset-library/releases
2. Your new release should appear at the top
3. Verify the changelog is populated

## Installation Commands for Users

After release, users can install with:

**Specific version:**
```json
{
  "dependencies": {
    "com.crossproject.assetlibrary": "https://github.com/AFreoN/asset-library.git#v0.2.0"
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

## Tag Format Rules

⚠️ **Important**: Tags MUST be in format `vX.Y.Z`

✅ Correct:
- `v0.1.0`
- `v1.0.0`
- `v0.2.1`

❌ Wrong:
- `0.1.0` (missing 'v')
- `version-0.1.0`
- `v0.1` (incomplete)

## Troubleshooting

**Release didn't appear?**
```bash
# Check if tag was pushed
git tag -l

# Check GitHub Actions
# Go to Actions tab in repository and look for failed workflows
```

**Need to fix a release?**
```bash
# Delete tag locally and remotely
git tag -d v0.2.0
git push origin :refs/tags/v0.2.0

# Create again with correct info
git tag -a v0.2.0 -m "Release version 0.2.0"
git push origin v0.2.0
```

## Automation

The workflow file is at `.github/workflows/release.yml` and:
- Activates when you push a tag matching `v*.*.*`
- Parses your CHANGELOG.md automatically
- Creates a GitHub Release with changelog as description
- Handles everything - no manual GitHub Release creation needed

## For Full Details

See:
- [RELEASING.md](RELEASING.md) - Detailed release guide
- [CONTRIBUTING.md](CONTRIBUTING.md) - Development guide
- [INSTALL.md](INSTALL.md) - Installation methods
