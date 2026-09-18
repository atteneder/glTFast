// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cloud.Gltfast.Export;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
// MenuItem are designed for `static void` signatures, but `async Task` is required for the async export methods.
// ReSharper disable Unity.IncorrectMethodSignature

#if GLTF_VALIDATOR
using UnityEditor.Formats.Gltf.Validation;
#endif

namespace Unity.Cloud.Gltfast.Editor
{

    using Logging;

    static class MenuEntries
    {
        static string SaveFolderPath
        {
            get
            {
                var saveFolderPath = EditorUserSettings.GetConfigValue("glTF.saveFilePath");
                if (string.IsNullOrEmpty(saveFolderPath))
                {
                    saveFolderPath = Application.streamingAssetsPath;
                }
                return saveFolderPath;
            }
            set => EditorUserSettings.SetConfigValue("glTF.saveFilePath", value);
        }

        [MenuItem("Assets/Export glTF/glTF (.gltf)", true)]
        static bool ExportSelectionValidate()
        {
            return TryGetExportNameAndGameObjects(out _, out _);
        }

        [MenuItem("Assets/Export glTF/glTF (.gltf)", false, 31)]
        static async Task ExportSelectionMenu()
        {
            try
            {
                await ExportSelection(false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [MenuItem("Assets/Export glTF/glTF-Binary (.glb)", true)]
        static bool ExportSelectionBinaryValidate()
        {
            return TryGetExportNameAndGameObjects(out _, out _);
        }

        [MenuItem("Assets/Export glTF/glTF-Binary (.glb)", false, 32)]
        static async Task ExportSelectionBinaryMenu()
        {
            try
            {
                await ExportSelection(true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [MenuItem("GameObject/Export glTF/glTF (.gltf)", true)]
        static bool ExportGameObjectValidate()
        {
            return TryGetExportNameAndGameObjects(out _, out _);
        }

        [MenuItem("GameObject/Export glTF/glTF (.gltf)", false, 32)]
        static async Task ExportGameObjectMenu(MenuCommand command)
        {
            try
            {
                await ExportGameObject(command, false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [MenuItem("GameObject/Export glTF/glTF-Binary (.glb)", true)]
        static bool ExportGameObjectBinaryValidate()
        {
            return TryGetExportNameAndGameObjects(out _, out _);
        }

        [MenuItem("GameObject/Export glTF/glTF-Binary (.glb)", false, 31)]
        static async Task ExportGameObjectBinaryMenu(MenuCommand command)
        {
            try
            {
                await ExportGameObject(command, true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        static async Task ExportGameObject(MenuCommand command, bool binary)
        {
            var go = command.context as GameObject;
            if (go != null)
            {
                await Export(binary, go.name, new[] { go });
            }
            else if (TryGetExportNameAndGameObjects(out var name, out var gameObjects))
            {
                await Export(binary, name, gameObjects);
            }
        }

        static async Task ExportSelection(bool binary)
        {
            if (TryGetExportNameAndGameObjects(out var name, out var gameObjects))
            {
                await Export(binary, name, gameObjects);
            }
            else
            {
                Debug.LogError("Can't export glTF: selection is empty");
            }
        }

        static async Task Export(bool binary, string name, GameObject[] gameObjects)
        {
            var extension = binary ? Constants.gltfBinaryExtension : Constants.gltfExtension;
            var path = EditorUtility.SaveFilePanel(
                "glTF Export Path",
                SaveFolderPath,
                $"{name}.{extension}",
                extension
            );
            if (!string.IsNullOrEmpty(path))
            {
                await Export(path, binary, name, gameObjects);
            }
        }

        internal static async Task Export(string destinationPath, bool binary, string name, GameObject[] gameObjects)
        {
            SaveFolderPath = Directory.GetParent(destinationPath)?.FullName;
            var settings = GetDefaultSettings(binary);
            var goSettings = new GameObjectExportSettings { OnlyActiveInHierarchy = false };
            var export = new GameObjectExport(settings, gameObjectExportSettings: goSettings);
            export.AddScene(gameObjects, name);
#if GLTF_VALIDATOR
            var success =
#endif
            await export.SaveToFileAndDisposeAsync(destinationPath);

#if GLTF_VALIDATOR
            if (success)
            {
                var report = Validator.Validate(destinationPath);
                report.Log();
            }
#endif
        }

        static ExportSettings GetDefaultSettings(bool binary)
        {
            var settings = new ExportSettings
            {
                Format = binary ? GltfFormat.Binary : GltfFormat.Json
            };
            return settings;
        }

        [MenuItem("File/Export Scene/glTF (.gltf)", false, 173)]
        static async Task ExportSceneMenu()
        {
            try
            {
                await ExportScene(false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [MenuItem("File/Export Scene/glTF-Binary (.glb)", false, 174)]
        static async Task ExportSceneBinaryMenu()
        {
            try
            {
                await ExportScene(true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        static async Task ExportScene(bool binary)
        {
            var scene = SceneManager.GetActiveScene();
            var gameObjects = scene.GetRootGameObjects();
            var extension = binary ? Constants.gltfBinaryExtension : Constants.gltfExtension;

            var path = EditorUtility.SaveFilePanel(
                "glTF Export Path",
                SaveFolderPath,
                $"{scene.name}.{extension}",
                extension
                );
            if (!string.IsNullOrEmpty(path))
            {
                SaveFolderPath = Directory.GetParent(path)?.FullName;
                var settings = GetDefaultSettings(binary);
                var export = new GameObjectExport(settings);
                export.AddScene(gameObjects, scene.name);
                await export.SaveToFileAndDisposeAsync(path);
#if GLTF_VALIDATOR
                var report = Validator.Validate(path);
                report.Log();
#endif
            }
        }

        static bool TryGetExportNameAndGameObjects(out string name, out GameObject[] gameObjects)
        {
            var transforms = Selection.GetTransforms(SelectionMode.Assets | SelectionMode.TopLevel);
            if (transforms.Length > 0)
            {
                name = transforms.Length > 1
                    ? SceneManager.GetActiveScene().name
                    : Selection.activeObject.name;

                gameObjects = transforms.Select(x => x.gameObject).ToArray();
                return true;
            }

            name = null;
            gameObjects = null;
            return false;
        }
    }
}
