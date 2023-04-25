using System;
using GLTFast.Utils;
using GLTFast.Logging;
using GLTFast.Export;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GLTFast.Editor
{
    public static class Exporter
    {
        static readonly string k_GltfExtension = "gltf";
        static readonly string k_GltfBinaryExtension = "glb";

        public static bool ExportScene(
            bool binary,
            string exportFolderPath,
            string sceneName,
            ExportSettings exportSettings = null,
            GameObjectExportSettings gameObjectExportSettings = null
        )
        {
            var scene = SceneManager.GetActiveScene();
            var gameObjects = scene.GetRootGameObjects();
            var ext = binary ? k_GltfBinaryExtension : k_GltfExtension;
            if (exportSettings == null)
                exportSettings = new ExportSettings
                {
                    Format = binary ? GltfFormat.Binary : GltfFormat.Json
                };

            if (gameObjectExportSettings == null)
                gameObjectExportSettings = new GameObjectExportSettings { };

            try
            {
                System.IO.Directory.CreateDirectory(exportFolderPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating export folder: {e}");
            }

            var path = System.IO.Path.Combine(exportFolderPath, $"{sceneName}.{ext}");

            bool success = false;
            if (!string.IsNullOrEmpty(path))
            {
                var export = new GameObjectExport(
                    exportSettings,
                    gameObjectExportSettings,
                    logger: new ConsoleLogger()
                );
                export.AddScene(gameObjects, sceneName);
                success = AsyncHelpers.RunSync(() => export.SaveToFileAndDispose(path));
            }

            return success;
        }
    }
}
