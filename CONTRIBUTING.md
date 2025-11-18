# Contributing to Cross Project Asset Library

Thank you for your interest in contributing! This document outlines the process for contributing to the project.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/asset-library.git
   cd asset-library
   ```
3. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```
4. **Open the project** in Unity 2020.3 LTS or newer

## Development Workflow

### Code Style

- Follow standard C# conventions
- Use consistent indentation (4 spaces)
- Add XML documentation comments for public methods/classes
- Use `namespace CPAL` for all code

Example:
```csharp
namespace CPAL
{
    /// <summary>
    /// Loads a Unity library file from disk.
    /// </summary>
    /// <param name="libraryPath">Path to the .unitylib file</param>
    /// <returns>The loaded library manifest</returns>
    public static LibraryManifest LoadLibrary(string libraryPath)
    {
        // Implementation...
    }
}
```

### File Organization

- **Data Layer**: `Editor/Scripts/Data/` - Serializable models
- **Core Layer**: `Editor/Scripts/Core/` - Business logic
- **UI Layer**: `Editor/Scripts/UI/` - Editor windows and dialogs
- **Utilities**: `Editor/Scripts/Utilities/` - Helper functions

### Making Changes

1. **Implement your feature or fix**
2. **Test thoroughly in the Unity Editor**
3. **Update documentation** if needed
4. **Update CHANGELOG.md** with your changes

Example CHANGELOG entry:
```markdown
### Added
- New feature description

### Fixed
- Bug fix description
```

### Commit Messages

Use clear, descriptive commit messages:

```
Add feature: brief description

Longer explanation if needed. Reference issues using #123.
```

Examples:
- `Add: Asset preview functionality for 3D models`
- `Fix: Library loading crash on Windows paths with spaces`
- `Update: Improve search performance with indexed queries`

## Testing

### Manual Testing (Current MVP)

1. **Create a test Unity project** (2020.3 LTS or newer)
2. **Copy the package** to `Assets/Packages/com.crossproject.assetlibrary/`
3. **Test your changes**:
   - Create a library
   - Add assets with various types
   - Edit metadata
   - Import assets
   - Use search and filtering
   - Test on different platforms if possible

### Testing Checklist

- [ ] Feature works as designed
- [ ] No compilation errors
- [ ] No runtime errors in console
- [ ] Existing functionality still works
- [ ] UI is responsive
- [ ] File operations complete successfully

## Pull Request Process

1. **Push your branch** to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Create a Pull Request** on GitHub with:
   - Clear title describing the change
   - Description of what was changed and why
   - Reference any related issues (#123)
   - Screenshots for UI changes

3. **PR Title Format**:
   ```
   [Type] Brief description

   Examples:
   - [Feature] Add cloud storage integration
   - [Fix] Resolve library loading issue
   - [Docs] Update installation instructions
   ```

4. **Include Testing Notes**:
   ```markdown
   ## Testing
   - [ ] Tested library creation
   - [ ] Tested asset import
   - [ ] Verified on Windows
   - [ ] No console errors
   ```

5. **Respond to feedback** and make requested changes

## Reporting Issues

When reporting bugs:

1. **Describe the issue** clearly
2. **Include steps to reproduce**
3. **Provide screenshots** if applicable
4. **List environment** (Unity version, OS, etc.)

Example:
```
**Title**: Library crashes when importing prefabs with spaces in names

**Environment**:
- Unity 2021.3.1f1
- Windows 10

**Steps to Reproduce**:
1. Create a library
2. Add a prefab named "My Test Prefab"
3. Try to import it
4. Crash occurs

**Expected**: Import succeeds
**Actual**: NullReferenceException in AssetImporter.cs:45

**Additional Notes**: Works fine with names without spaces
```

## Feature Requests

When suggesting features:

1. **Describe the use case**
2. **Explain the benefit**
3. **Provide examples** if applicable
4. **Consider alternatives**

Example:
```
**Title**: Add asset dependency tracking

**Description**: Users need to know which assets depend on other assets

**Use Case**: When deleting an asset, show warnings if other assets reference it

**Benefits**:
- Prevents accidental deletion of critical assets
- Helps understand asset relationships
```

## Documentation

### Updating README.md

- Keep the quick start section up-to-date
- Update feature list when adding features
- Update limitations when resolving known issues

### CHANGELOG.md Format

Follow [Keep a Changelog](https://keepachangelog.com/):

```markdown
## [0.2.0] - 2025-01-15

### Added
- New features

### Changed
- Modifications to existing features

### Fixed
- Bug fixes

### Removed
- Deprecated features

### Security
- Security updates
```

## Versioning

The project uses [Semantic Versioning](https://semver.org/):

- **MAJOR** (X.0.0): Breaking changes
- **MINOR** (0.X.0): New backward-compatible features
- **PATCH** (0.0.X): Bug fixes

## Release Process

See [RELEASING.md](RELEASING.md) for the release process.

## Code Review

All contributions go through code review:

1. **Maintainers** review your PR
2. **Feedback** is provided if needed
3. **Approval** comes when PR is ready
4. **Merge** happens after approval

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Questions?

- Open a GitHub Discussion
- Open an Issue for clarification
- Check existing documentation

---

Thank you for contributing to Cross Project Asset Library!
