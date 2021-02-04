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
using UnityEditor;
using UnityEngine;

#if GLTFAST_RENDER_TEST
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using GLTFast;
using GLTFast.Schema;
using GLTFast.Tests;
using UnityEditor.SceneManagement;
#endif

namespace GLTFast.Editor {

    using Samples;

    [CustomEditor(typeof(SampleSet))]
    public class SampleSetEditor : UnityEditor.Editor
    {
        private SampleSet _sampleSet;
        private string searchPattern = "*.gl*";

        public void OnEnable() {
            _sampleSet = (SampleSet)target;
        }

        public override void OnInspectorGUI() {
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search Pattern");
            searchPattern = GUILayout.TextField(searchPattern);
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Find in path")) {
                _sampleSet.LoadItemsFromPath(searchPattern);
            }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Activate All")) {
                _sampleSet.SetAllActive();
            }
            if (GUILayout.Button("Deactivate All")) {
                _sampleSet.SetAllActive(false);
            }
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Create JSONs")) {
                CreateJSON(_sampleSet,target);
            }
            
            if (GUILayout.Button("Create render test scenes")) {
                CreateRenderTestScenes(_sampleSet);
            }
            
            base.OnInspectorGUI();
            
            if (GUI.changed) {
                EditorUtility.SetDirty(_sampleSet);
            }
        }
        
        public static void CreateRenderTestScenes(SampleSet sampleSet)
        {
    #if GLTFAST_RENDER_TEST
            var allScenes = new List<EditorBuildSettingsScene>();
            Texture2D dummyReference = null;

            foreach (var item in sampleSet.GetItems())
            {
                var testScene = EditorSceneManager.OpenScene("Assets/Scenes/TestScene.unity");
                
                var settingsGameObject = new GameObject("GraphicsTestSettings");
                var graphicsTestSettings = settingsGameObject.AddComponent<UniversalGraphicsTestSettings>();

                var go = new GameObject(item.name);
                var gltfAsset = go.AddComponent<GltfBoundsAsset>();
                
                if(string.IsNullOrEmpty(sampleSet.streamingAssetsPath)) {
                    gltfAsset.url = Path.Combine(sampleSet.baseLocalPath, item.path);
                } else {
                    gltfAsset.url = Path.Combine(sampleSet.streamingAssetsPath, item.path);
                    gltfAsset.streamingAsset = true;
                }
                gltfAsset.loadOnStartup = true;
                gltfAsset.createBoxCollider = false;
                
                var sceneDirectory = CertifyDirectory(item.directoryParts, string.Format("Assets/Scenes/{0}", sampleSet.name));
                var scenePath = Path.Combine(sceneDirectory, item.name+".unity");

                EditorSceneManager.SaveScene(testScene,scenePath);
                allScenes.Add(new EditorBuildSettingsScene(scenePath,true));

                var referenceImagePath =
                    Path.Combine(Application.dataPath, "ReferenceImages/Linear/OSXEditor/Metal/None", item.name + ".png");
                if (!File.Exists(referenceImagePath)) {
                    Debug.LogFormat("Create dummy reference at path {0}", referenceImagePath);
                    dummyReference = dummyReference!=null
                        ? dummyReference
                        : new Texture2D(
                        graphicsTestSettings.ImageComparisonSettings.TargetWidth,
                        graphicsTestSettings.ImageComparisonSettings.TargetHeight
                    );
                    File.WriteAllBytes(referenceImagePath, dummyReference.EncodeToPNG());
                }
            }
            AssetDatabase.Refresh();
            EditorBuildSettings.scenes = allScenes.ToArray();
    #else
            Debug.LogWarning("Please install  the Graphics Test Framework for render tests to work.");
    #endif
        }

        public static void CreateJSON(SampleSet sampleSet, Object target) {
            var jsonPathAbsolute = Path.Combine( Application.streamingAssetsPath, $"{sampleSet.name}.json");
            Debug.Log(jsonPathAbsolute);
            var json = JsonUtility.ToJson(sampleSet);
            File.WriteAllText(jsonPathAbsolute,json);
        }

        private static string CertifyDirectory(string[] directoryParts, string directoyPath)
        {
            foreach (var dirPart in directoryParts)
            {
                var newFolder = Path.Combine(directoyPath, dirPart);
                if (!AssetDatabase.IsValidFolder(newFolder))
                {
                    AssetDatabase.CreateFolder(directoyPath, dirPart);
                }

                directoyPath = newFolder;
            }

            return directoyPath;
        }
    }
}
