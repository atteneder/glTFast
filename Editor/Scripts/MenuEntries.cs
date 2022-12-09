// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.IO;
using System.Linq;
using GLTFast.Export;
using GLTFast.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

#if GLTF_VALIDATOR
using Unity.glTF.Validator;
#endif

namespace GLTFast.Editor
{

    using Logging;

    static class MenuEntries
    {

        const string k_GltfExtension = "gltf";
        const string k_GltfBinaryExtension = "glb";

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
        static void ExportSelectionMenu()
        {
            ExportSelection(false);
        }

        [MenuItem("Assets/Export glTF/glTF-Binary (.glb)", true)]
        static bool ExportSelectionBinaryValidate()
        {
            return TryGetExportNameAndGameObjects(out _, out _);
        }

        [MenuItem("Assets/Export glTF/glTF-Binary (.glb)", false, 32)]
        static void ExportSelectionBinaryMenu()
        {
            ExportSelection(true);
        }

        [MenuItem("GameObject/Export glTF/glTF (.gltf)", true)]
        static bool ExportGameObjectValidate()
        {
            return TryGetExportNameAndGameObjects(out _, out _);
        }

        [MenuItem("GameObject/Export glTF/glTF (.gltf)", false, 32)]
        static void ExportGameObjectMenu(MenuCommand command)
        {
            ExportGameObject(command, false);
        }

        [MenuItem("GameObject/Export glTF/glTF-Binary (.glb)", true)]
        static bool ExportGameObjectBinaryValidate()
        {
            return TryGetExportNameAndGameObjects(out _, out _);
        }

        [MenuItem("GameObject/Export glTF/glTF-Binary (.glb)", false, 31)]
        static void ExportGameObjectBinaryMenu(MenuCommand command)
        {
            ExportGameObject(command, true);
        }

        static void ExportGameObject(MenuCommand command, bool binary)
        {
            var go = command.context as GameObject;
            if (go != null)
            {
                Export(binary, go.name, new[] { go });
            }
            else if (TryGetExportNameAndGameObjects(out var name, out var gameObjects))
            {
                Export(binary, name, gameObjects);
            }
        }

        static void ExportSelection(bool binary)
        {
            if (TryGetExportNameAndGameObjects(out var name, out var gameObjects))
            {
                Export(binary, name, gameObjects);
            }
            else
            {
                Debug.LogError("Can't export glTF: selection is empty");
            }
        }

        static void Export(bool binary, string name, GameObject[] gameObjects)
        {
            var extension = binary ? k_GltfBinaryExtension : k_GltfExtension;
            var path = EditorUtility.SaveFilePanel(
                "glTF Export Path",
                SaveFolderPath,
                $"{name}.{extension}",
                extension
            );
            if (!string.IsNullOrEmpty(path))
            {
                SaveFolderPath = Directory.GetParent(path)?.FullName;
                var settings = GetDefaultSettings(binary);
                var goSettings = new GameObjectExportSettings { OnlyActiveInHierarchy = false };
                var export = new GameObjectExport(settings, gameObjectExportSettings: goSettings, logger: new ConsoleLogger());
                export.AddScene(gameObjects, name);
                AsyncHelpers.RunSync(() => export.SaveToFileAndDispose(path));

#if GLTF_VALIDATOR
                var report = Validator.Validate(path);
                report.Log();
#endif
            }
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
        static void ExportSceneMenu()
        {
            ExportScene(false);
        }

        [MenuItem("File/Export Scene/glTF-Binary (.glb)", false, 174)]
        static void ExportSceneBinaryMenu()
        {
            ExportScene(true);
        }

        static void ExportScene(bool binary)
        {
            var scene = SceneManager.GetActiveScene();
            var gameObjects = scene.GetRootGameObjects();
            var extension = binary ? k_GltfBinaryExtension : k_GltfExtension;

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
                var export = new GameObjectExport(settings, logger: new ConsoleLogger());
                export.AddScene(gameObjects, scene.name);
                AsyncHelpers.RunSync(() => export.SaveToFileAndDispose(path));
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
