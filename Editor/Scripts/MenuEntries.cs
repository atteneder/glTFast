// Copyright 2020-2021 Andreas Atteneder
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
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

#if GLTF_VALIDATOR
using Unity.glTF.Validator;
#endif

namespace GLTFast.Editor {
    
    public static class MenuEntries {

        const string k_GltfExtension = "gltf";
        const string k_GltfBinaryExtension = "glb";

        static string SaveFolderPath {
            get {
                var saveFolderPath = EditorUserSettings.GetConfigValue("glTF.saveFilePath");
                if (string.IsNullOrEmpty(saveFolderPath)) {
                    saveFolderPath = Application.streamingAssetsPath;
                }
                return saveFolderPath;
            }
            set => EditorUserSettings.SetConfigValue("glTF.saveFilePath",value);
        }

        [MenuItem("File/Export/Selection (glTF)", true)]
        static bool ExportSelectionValidate() {
            return TryGetExportNameAndGameObjects(out _, out _);
        }

        [MenuItem("File/Export/Selection (glTF)", false, 10)]
        static void ExportSelectionMenu() {
            ExportSelection(false);
        }

        [MenuItem("File/Export/Selection (glTF-Binary)", true)]
        static bool ExportSelectionBinaryValidate() {
            return TryGetExportNameAndGameObjects(out _, out _);
        }

        [MenuItem("File/Export/Selection (glTF-Binary)", false, 11)]
        static void ExportSelectionBinaryMenu() {
            ExportSelection(true);
        }

        static void ExportSelection(bool binary) {
            if (TryGetExportNameAndGameObjects(out var name, out var gameObjects)) {
                var extension = binary ? k_GltfBinaryExtension : k_GltfExtension;
                var path = EditorUtility.SaveFilePanel(
                    "glTF Export Path",
                    SaveFolderPath,
                    $"{name}.{extension}",
                    extension
                    );
                if (!string.IsNullOrEmpty(path)) {
                    var settings = GetDefaultSettings(binary);
                    var export = new GameObjectExport(settings, logger: new ConsoleLogger());
                    export.AddScene(gameObjects, name);
                    AsyncHelpers.RunSync(() => export.SaveToFileAndDispose(path));

#if GLTF_VALIDATOR
                    var report = Validator.Validate(path);
                    report.Log();
#endif
                }
            }
            else {
                Debug.LogError("Can't export glTF: selection is empty");
            }
        }

        static ExportSettings GetDefaultSettings(bool binary) {
            var settings = new ExportSettings {
                format = binary ? GltfFormat.Binary : GltfFormat.Json
            };
            return settings;
        }

        [MenuItem("File/Export/Scene (glTF)", false, 100)]
        static void ExportSceneMenu() {
            ExportScene(false);
        }

        [MenuItem("File/Export/Scene (glTF-Binary)", false, 101)]
        static void ExportSceneBinaryMenu() {
            ExportScene(true);
        }

        static void ExportScene(bool binary) {
            var scene = SceneManager.GetActiveScene();
            var gameObjects = scene.GetRootGameObjects();
            var extension = binary ? k_GltfBinaryExtension : k_GltfExtension;
            
            var path = EditorUtility.SaveFilePanel(
                "glTF Export Path",
                SaveFolderPath, 
                $"{scene.name}.{extension}", 
                extension
                );
            if (!string.IsNullOrEmpty(path)) {
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
            if (Selection.transforms.Length > 1) {
                name = SceneManager.GetActiveScene().name;
                gameObjects = Selection.gameObjects;
                return true;
            }

            if (Selection.transforms.Length == 1) {
                name = Selection.activeGameObject.name;
                gameObjects = Selection.gameObjects;
                return true;
            }

            if (Selection.objects.Any() && Selection.objects.All(x => x is GameObject)) {
                name = Selection.objects.First().name;
                gameObjects = Selection.objects.Select(x => (x as GameObject)).ToArray();
                return true;
            }

            name = null;
            gameObjects = null;
            return false;
        }
    }
}
