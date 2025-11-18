using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CPAM
{
    /// <summary>
    /// Handles drag and drop operations from the asset library window to the project.
    /// </summary>
    public static class DragAndDropHandler
    {
        /// <summary>
        /// Handle drag and drop event in the asset library window.
        /// Should be called during OnGUI when processing drag and drop events.
        /// </summary>
        public static void HandleDragAndDrop(AssetLibraryLoader loader, List<AssetMetadata> draggedAssets)
        {
            if (loader == null || !loader.IsLoaded || draggedAssets == null || draggedAssets.Count == 0)
            {
                return;
            }

            var evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    // Set drag operation
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        PerformDragAndDrop(loader, draggedAssets);
                        evt.Use();
                    }
                    break;
            }
        }

        /// <summary>
        /// Perform the actual drag and drop import.
        /// </summary>
        private static void PerformDragAndDrop(AssetLibraryLoader loader, List<AssetMetadata> draggedAssets)
        {
            try
            {
                AssetImporter.ImportAssets(loader, draggedAssets);
            }
            catch (Exception ex)
            {
                LibraryUtilities.LogError($"Drag and drop import failed: {ex.Message}");
            }
        }
    }
}
