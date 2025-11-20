using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace CPAL
{
    /// <summary>
    /// Handles all operations on .unitylib files (ZIP-based archives).
    /// Provides methods to extract, compress, and manipulate library files.
    /// </summary>
    public static class UnityLibFileHandler
    {
        /// <summary>
        /// The manifest filename within the library.
        /// </summary>
        public const string ManifestFileName = "manifest.json";

        /// <summary>
        /// The assets folder name within the library.
        /// </summary>
        public const string AssetsFolderName = "assets";

        /// <summary>
        /// The thumbnails folder name within the library.
        /// </summary>
        public const string ThumbnailsFolderName = "thumbnails";

        /// <summary>
        /// Extract a .unitylib file to a temporary directory.
        /// </summary>
        /// <returns>The path to the extracted directory, or null if extraction failed.</returns>
        public static string ExtractLibrary(string libraryPath)
        {
            try
            {
                if (!LibraryUtilities.IsValidLibraryFile(libraryPath))
                {
                    LibraryUtilities.LogError($"Invalid library file: {libraryPath}");
                    return null;
                }

                var extractPath = LibraryUtilities.CreateLibraryTempDirectory(
                    Path.GetFileNameWithoutExtension(libraryPath)
                );

                using (var zipArchive = ZipFile.OpenRead(libraryPath))
                {
                    zipArchive.ExtractToDirectory(extractPath, overwriteFiles: true);
                }

                LibraryUtilities.Log($"Successfully extracted library to: {extractPath}");
                return extractPath;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to extract library: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Read the manifest JSON from an extracted library directory.
        /// </summary>
        public static LibraryManifest ReadManifest(string extractedLibraryPath)
        {
            try
            {
                var manifestPath = Path.Combine(extractedLibraryPath, ManifestFileName);

                if (!File.Exists(manifestPath))
                {
                    LibraryUtilities.LogError($"Manifest file not found: {manifestPath}");
                    return null;
                }

                var json = File.ReadAllText(manifestPath, Encoding.UTF8);
                var manifest = JsonUtility.FromJson<LibraryManifest>(json);

                if (manifest == null)
                {
                    LibraryUtilities.LogError("Failed to deserialize manifest JSON");
                    return null;
                }

                return manifest;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to read manifest: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Write a manifest to a directory (as JSON).
        /// </summary>
        public static bool WriteManifest(string directoryPath, LibraryManifest manifest)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                manifest.UpdateModifiedDate();
                var json = JsonUtility.ToJson(manifest, prettyPrint: true);
                var manifestPath = Path.Combine(directoryPath, ManifestFileName);

                File.WriteAllText(manifestPath, json, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to write manifest: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Compress a directory into a .unitylib file.
        /// </summary>
        public static bool CompressLibrary(string sourceDirectory, string outputLibraryPath, System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal)
        {
            try
            {
                // Ensure output directory exists
                var outputDir = Path.GetDirectoryName(outputLibraryPath);
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                // Remove existing file if it exists
                if (File.Exists(outputLibraryPath))
                {
                    try
                    {
                        File.Delete(outputLibraryPath);
                    }
                    catch (IOException ex)
                    {
                        LibraryUtilities.LogWarning($"Failed to delete existing library file (file locked): {ex.Message}");
                        LibraryUtilities.LogWarning("Attempting to use temporary file and replace...");

                        // Try alternative approach: create to temp file first, then replace
                        string tempOutputPath = outputLibraryPath + ".tmp";
                        if (File.Exists(tempOutputPath))
                        {
                            File.Delete(tempOutputPath);
                        }

                        ZipFile.CreateFromDirectory(sourceDirectory, tempOutputPath, compressionLevel, includeBaseDirectory: false);

                        // Try to replace the original file
                        int maxRetries = 5;
                        int retryCount = 0;
                        while (retryCount < maxRetries)
                        {
                            try
                            {
                                if (File.Exists(outputLibraryPath))
                                {
                                    File.Delete(outputLibraryPath);
                                }
                                File.Move(tempOutputPath, outputLibraryPath);
                                LibraryUtilities.Log($"Successfully compressed library to: {outputLibraryPath} (Level: {compressionLevel})");
                                return true;
                            }
                            catch (IOException)
                            {
                                retryCount++;
                                if (retryCount < maxRetries)
                                {
                                    System.Threading.Thread.Sleep(100); // Wait before retry
                                }
                            }
                        }

                        // If replacement failed, clean up temp file
                        if (File.Exists(tempOutputPath))
                        {
                            File.Delete(tempOutputPath);
                        }

                        throw ex;
                    }
                }

                // Create ZIP archive with specified compression level
                ZipFile.CreateFromDirectory(sourceDirectory, outputLibraryPath, compressionLevel, includeBaseDirectory: false);

                LibraryUtilities.Log($"Successfully compressed library to: {outputLibraryPath} (Level: {compressionLevel})");
                return true;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to compress library: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get a file from the extracted library.
        /// Returns null if the file doesn't exist.
        /// </summary>
        public static byte[] ReadFileFromLibrary(string extractedLibraryPath, string relativePath)
        {
            try
            {
                var filePath = Path.Combine(extractedLibraryPath, relativePath);

                if (!File.Exists(filePath))
                {
                    LibraryUtilities.LogWarning($"File not found in library: {relativePath}");
                    return null;
                }

                return File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to read file from library: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Copy a file into the extracted library at the given relative path.
        /// </summary>
        public static bool WriteFileToLibrary(string extractedLibraryPath, string sourcePath, string relativePath)
        {
            try
            {
                var destinationPath = Path.Combine(extractedLibraryPath, relativePath);
                var destinationDir = Path.GetDirectoryName(destinationPath);

                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                File.Copy(sourcePath, destinationPath, overwrite: true);
                return true;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to write file to library: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a temporary extracted library directory.
        /// </summary>
        public static bool DeleteTemporaryDirectory(string directoryPath)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, recursive: true);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogWarning($"Failed to delete temporary directory: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a file exists in the extracted library.
        /// </summary>
        public static bool FileExistsInLibrary(string extractedLibraryPath, string relativePath)
        {
            var filePath = Path.Combine(extractedLibraryPath, relativePath);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Get the size of a file in the extracted library.
        /// </summary>
        public static long GetFileSizeInLibrary(string extractedLibraryPath, string relativePath)
        {
            try
            {
                var filePath = Path.Combine(extractedLibraryPath, relativePath);
                if (File.Exists(filePath))
                {
                    return new FileInfo(filePath).Length;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Create a new empty library file.
        /// </summary>
        public static bool CreateNewLibrary(string libraryPath, string libraryName)
        {
            try
            {
                var tempDir = LibraryUtilities.CreateLibraryTempDirectory(libraryName);

                // Create manifest
                var manifest = new LibraryManifest(libraryName);

                // Create required directories
                Directory.CreateDirectory(Path.Combine(tempDir, AssetsFolderName));
                Directory.CreateDirectory(Path.Combine(tempDir, ThumbnailsFolderName));

                // Write manifest
                if (!WriteManifest(tempDir, manifest))
                {
                    DeleteTemporaryDirectory(tempDir);
                    return false;
                }

                // Compress to .unitylib
                if (!CompressLibrary(tempDir, libraryPath))
                {
                    DeleteTemporaryDirectory(tempDir);
                    return false;
                }

                // Clean up temp directory
                DeleteTemporaryDirectory(tempDir);
                return true;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to create new library: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get list of all files in a directory within the extracted library.
        /// </summary>
        public static List<string> GetFilesInLibraryDirectory(string extractedLibraryPath, string relativeDirPath)
        {
            var files = new List<string>();

            try
            {
                var dirPath = Path.Combine(extractedLibraryPath, relativeDirPath);

                if (!Directory.Exists(dirPath))
                {
                    return files;
                }

                var allFiles = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories);

                foreach (var file in allFiles)
                {
                    var relativePath = Path.GetRelativePath(extractedLibraryPath, file);
                    files.Add(relativePath.Replace(Path.DirectorySeparatorChar, '/'));
                }

                return files;
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Failed to get files in library directory: {ex.Message}");
                return files;
            }
        }
    }
}
