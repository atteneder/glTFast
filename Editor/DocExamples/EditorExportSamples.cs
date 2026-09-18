// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using GLTFast.Export;
using GLTFast.Logging;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GLTFast.Editor.Documentation.Examples
{
    static class EditorExportSamples
    {
        #region BatchExportAllObjects
        [MenuItem("Tools/glTFast Examples/Batch Export")]
        static async Task BatchExportAllObjects()
        {
            var currentScene = SceneManager.GetActiveScene();
            var rootObjects = currentScene.GetRootGameObjects();

            var exportSettings = new ExportSettings
            {
                Format = GltfFormat.Binary
            };

            var logger = new ConsoleLogger();
            try
            {
                for (var index = 0; index < rootObjects.Length; index++)
                {
                    var rootObject = rootObjects[index];

                    EditorUtility.DisplayProgressBar(
                        "glTF Batch Export", rootObject.name, index / (float)rootObjects.Length);

                    var export = new GameObjectExport(exportSettings, logger: logger);
                    export.AddScene(new[] { rootObject }, rootObject.name);

                    var success = await export.SaveToFileAndDispose(
                        $"Assets/{rootObject.name}.glb",
                        // Edit Mode does not pump the main-thread SynchronizationContext;
                        // awaited I/O continuations could hang. Force the synchronous path.
                        forceSync: true
                        );
                    if (!success)
                    {
                        Debug.LogError($"Exporting {rootObject.name} failed.");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }
        #endregion BatchExportAllObjects
    }
}
