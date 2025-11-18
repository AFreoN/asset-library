using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace CPAL
{
    /// <summary>
    /// Lazy reader for .unitylib ZIP files that reads files on-demand without full extraction.
    /// This provides better performance for large libraries by avoiding full disk extraction.
    /// </summary>
    public class LazyZipLibraryReader : IDisposable
    {
        private string _libraryPath;
        private ZipArchive _zipArchive;
        private LibraryManifest _manifest;
        private bool _disposed = false;
        private object _readLock = new object(); // Thread safety for ZIP reads

        public string LibraryPath => _libraryPath;
        public LibraryManifest Manifest => _manifest;

        /// <summary>
        /// Opens a library file for lazy reading.
        /// The ZIP archive remains open for fast file access without full extraction.
        /// </summary>
        public static LazyZipLibraryReader Open(string libraryPath)
        {
            try
            {
                if (!File.Exists(libraryPath))
                {
                    LibraryUtilities.LogError($"Library file not found: {libraryPath}");
                    return null;
                }

                var reader = new LazyZipLibraryReader { _libraryPath = libraryPath };

                // Open the ZIP archive for reading
                reader._zipArchive = ZipFile.OpenRead(libraryPath);

                // Read manifest
                reader._manifest = reader.ReadManifest();
                if (reader._manifest == null)
                {
                    LibraryUtilities.LogError("Failed to read manifest from library");
                    reader.Dispose();
                    return null;
                }

                LibraryUtilities.Log($"Opened library for lazy reading: {libraryPath}");
                return reader;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to open library for lazy reading: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Read a file from the library without extracting the entire archive.
        /// Thread-safe for concurrent access from background threads.
        /// </summary>
        public byte[] ReadFile(string relativePath)
        {
            if (_disposed || _zipArchive == null)
            {
                LibraryUtilities.LogError("LazyZipLibraryReader has been disposed");
                return null;
            }

            try
            {
                // Normalize path separators for ZIP archive
                var zipPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');

                lock (_readLock) // Synchronize ZIP reads
                {
                    var entry = _zipArchive.GetEntry(zipPath);
                    if (entry == null)
                    {
                        LibraryUtilities.LogWarning($"File not found in library: {relativePath}");
                        return null;
                    }

                    using (var stream = entry.Open())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            return memoryStream.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to read file from library: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if a file exists in the library.
        /// </summary>
        public bool FileExists(string relativePath)
        {
            if (_disposed || _zipArchive == null)
            {
                return false;
            }

            try
            {
                var zipPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
                lock (_readLock)
                {
                    return _zipArchive.GetEntry(zipPath) != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the size of a file in the library.
        /// </summary>
        public long GetFileSize(string relativePath)
        {
            if (_disposed || _zipArchive == null)
            {
                return 0;
            }

            try
            {
                var zipPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
                lock (_readLock)
                {
                    var entry = _zipArchive.GetEntry(zipPath);
                    if (entry != null)
                    {
                        return entry.Length;
                    }
                }
            }
            catch { }

            return 0;
        }

        private LibraryManifest ReadManifest()
        {
            try
            {
                var manifestEntry = _zipArchive.GetEntry(UnityLibFileHandler.ManifestFileName);
                if (manifestEntry == null)
                {
                    LibraryUtilities.LogError("Manifest file not found in library");
                    return null;
                }

                using (var stream = manifestEntry.Open())
                {
                    using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                    {
                        var json = reader.ReadToEnd();
                        var manifest = JsonUtility.FromJson<LibraryManifest>(json);

                        if (manifest == null)
                        {
                            LibraryUtilities.LogError("Failed to deserialize manifest JSON");
                            return null;
                        }

                        return manifest;
                    }
                }
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to read manifest: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _zipArchive?.Dispose();
                _manifest = null;
                _disposed = true;
            }
        }
    }
}
