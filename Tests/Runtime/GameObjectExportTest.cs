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

using System;
using System.Collections;
using System.IO;
using System.Linq;
using GLTFast.Export;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

#if GLTF_VALIDATOR && UNITY_EDITOR
using Unity.glTF.Validator;
#endif

namespace GLTFast.Tests {
    
    [TestFixture]
    public class GameObjectExportTest {

        [OneTimeSetUp]
        public void SetupTest() {
            SceneManager.LoadScene("ExportScene", LoadSceneMode.Single);
        }

        [Test]
        public void SimpleTree() {

            var root = new GameObject("root");
            var childA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childA.name = "child A";
            var childB = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            childB.name = "child B";
            childA.transform.parent = root.transform;
            childB.transform.parent = root.transform;
            childB.transform.localPosition = new Vector3(1, 0, 0);

            var logger = new CollectingLogger();
            var export = new GameObjectExport(logger:logger);
            export.AddScene(new []{root}, "UnityScene");
            var path = Path.Combine(Application.persistentDataPath, "root.gltf");
            var success = export.SaveToFileAndDispose(path);
            Assert.IsTrue(success);
            AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(path, MessageCode.UNUSED_OBJECT);
#endif
        }
        
        [UnityTest]
        public IEnumerator ExportSceneGameObjectsJson() {
            yield return null;
            ExportSceneGameObjects(false);
        }

        [UnityTest]
        public IEnumerator ExportSceneGameObjectsBinary() {
            yield return null;
            ExportSceneGameObjects(true);
        }

        [UnityTest]
        public IEnumerator ExportSceneAllJson() {
            yield return null;
            ExportSceneAll(false);
        }
        
        [UnityTest]
        public IEnumerator ExportSceneAllBinary() {
            yield return null;
            ExportSceneAll(true);
        }
        
        [Test]
        public void MeshMaterialCombinationTest() {

            var mc1 = new GltfWriter.MeshMaterialCombination(42,new [] {1,2,3});
            var mc2 = new GltfWriter.MeshMaterialCombination(42,new [] {1,2,3});

            Assert.AreEqual(mc1,mc2);

            mc1 = new GltfWriter.MeshMaterialCombination(42,new [] {1,2,4});
            Assert.AreNotEqual(mc1,mc2);
            
            mc1 = new GltfWriter.MeshMaterialCombination(42,new [] {1,2});
            Assert.AreNotEqual(mc1,mc2);
            
            mc1 = new GltfWriter.MeshMaterialCombination(42,null);
            Assert.AreNotEqual(mc1,mc2);
            
            mc2 = new GltfWriter.MeshMaterialCombination(42,null);
            Assert.AreEqual(mc1,mc2);
            
            mc1 = new GltfWriter.MeshMaterialCombination(13,null);
            Assert.AreNotEqual(mc1,mc2);
        }
        
        [Test]
        public void TwoScenes() {

            var childA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childA.name = "child A";

            var logger = new CollectingLogger();
            var export = new GameObjectExport(logger:logger);
            export.AddScene(new []{childA}, "scene A");
            export.AddScene(new []{childA}, "scene B");
            var path = Path.Combine(Application.persistentDataPath, "TwoScenes.gltf");
            var success = export.SaveToFileAndDispose(path);
            Assert.IsTrue(success);
            AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(path, MessageCode.UNUSED_OBJECT);
#endif
        }
        
        [Test]
        public void Empty() {

            var logger = new CollectingLogger();
            var export = new GameObjectExport(logger:logger);
            var path = Path.Combine(Application.persistentDataPath, "Empty.gltf");
            var success = export.SaveToFileAndDispose(path);
            Assert.IsTrue(success);
            AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(path, MessageCode.UNUSED_OBJECT);
#endif
        }
        
        
        [Test]
        public void SavedTwice() {

            var childA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childA.name = "child A";

            var logger = new CollectingLogger();
            var export = new GameObjectExport(logger:logger);
            export.AddScene(new []{childA});
            var path = Path.Combine(Application.persistentDataPath, "SavedTwice1.gltf");
            var success = export.SaveToFileAndDispose(path);
            Assert.IsTrue(success);
            AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(path, MessageCode.UNUSED_OBJECT);
#endif
            Assert.Throws<InvalidOperationException>(delegate()
            {
                export.AddScene(new []{childA});
            });
            path = Path.Combine(Application.persistentDataPath, "SavedTwice2.gltf");
            Assert.Throws<InvalidOperationException>(delegate()
            {
                success = export.SaveToFileAndDispose(path);
            });
        }

        void ExportSceneGameObjects(bool binary) {
            var scene = SceneManager.GetActiveScene();

            var rootObjects = scene.GetRootGameObjects();

            Assert.AreEqual(21,rootObjects.Length);
            foreach (var gameObject in rootObjects) {
                var logger = new CollectingLogger();
                var export = new GameObjectExport(
                    new ExportSettings {
                        format = binary ? GltfFormat.Binary : GltfFormat.Json,
                        fileConflictResolution = FileConflictResolution.Overwrite,
                    },
                    logger
                );
                export.AddScene(new []{gameObject}, gameObject.name);
                var extension = binary ? GltfGlobals.glbExt : GltfGlobals.gltfExt;
                var path = Path.Combine(Application.persistentDataPath, $"{gameObject.name}{extension}");
                var success = export.SaveToFileAndDispose(path);
                Assert.IsTrue(success);
                AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
                ValidateGltf(path, new [] {
                    MessageCode.ACCESSOR_MAX_MISMATCH,
                    MessageCode.NODE_EMPTY,
                    MessageCode.UNUSED_OBJECT,
                });
#endif
            }
        }

        void ExportSceneAll(bool binary) {
            SceneManager.LoadScene("ExportScene", LoadSceneMode.Single);

            var scene = SceneManager.GetActiveScene();

            var rootObjects = scene.GetRootGameObjects();

            var logger = new CollectingLogger();
            var export = new GameObjectExport(
                new ExportSettings {
                    format = binary ? GltfFormat.Binary : GltfFormat.Json,
                    fileConflictResolution = FileConflictResolution.Overwrite,
                },
                logger
            );
            export.AddScene(rootObjects, "ExportScene");
            var extension = binary ? GltfGlobals.glbExt : GltfGlobals.gltfExt;
            var path = Path.Combine(Application.persistentDataPath, $"ExportScene{extension}");
            var success = export.SaveToFileAndDispose(path);
            Assert.IsTrue(success);
            AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(path, new [] {
                MessageCode.ACCESSOR_MAX_MISMATCH,
                MessageCode.NODE_EMPTY,
                MessageCode.UNUSED_OBJECT,
            });
#endif
        }

        void AssertLogger(CollectingLogger logger) {
            logger.LogAll();
            if (logger.items != null) {
                foreach (var item in logger.items) {
                    Assert.AreEqual(LogType.Log, item.type, item.ToString());
                }
            }
        }

#if GLTF_VALIDATOR && UNITY_EDITOR
        void ValidateGltf(string path, params MessageCode[] expectedMessages) {
            var report = Validator.Validate(path);
            Assert.NotNull(report, $"Report null for {path}");
            // report.Log();
            if (report.issues != null) {
                foreach (var message in report.issues.messages) {
                    if (expectedMessages.Contains(message.codeEnum)) {
                        continue;
                    }
                    Assert.Less(1, message.severity, $"Error {message} (path {Path.GetFileName(path)})");
                    Assert.Less(2, message.severity, $"Warning {message} (path {Path.GetFileName(path)})");
                }
            }
        }
#endif
    }
}
