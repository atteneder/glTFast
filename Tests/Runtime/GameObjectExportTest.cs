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

using System.Collections;
using System.IO;
using GLTFast.Export;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

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
            export.SaveToFile(Path.Combine(Application.persistentDataPath,"root.gltf"));
        }
        
        [UnityTest]
        public IEnumerator ExportSceneGameObjects() {
            
            yield return null;
            
            var scene = SceneManager.GetActiveScene();

            var rootObjects = scene.GetRootGameObjects();

            Assert.AreEqual(20,rootObjects.Length);
            foreach (var gameObject in rootObjects) {
                var logger = new CollectingLogger();
                var export = new GameObjectExport(
                    new ExportSettings {
                        fileConflictResolution = FileConflictResolution.Overwrite
                    },
                    logger
                    );
                export.AddScene(new []{gameObject}, gameObject.name);
                var success = export.SaveToFile(Path.Combine(Application.persistentDataPath,$"{gameObject.name}.gltf"));
                Assert.IsTrue(success);
            }
        }
        
        [UnityTest]
        public IEnumerator ExportSceneAll() {

            yield return null;
            
            SceneManager.LoadScene("ExportScene", LoadSceneMode.Single);

            var scene = SceneManager.GetActiveScene();

            var rootObjects = scene.GetRootGameObjects();

            var logger = new CollectingLogger();
            var export = new GameObjectExport(
                new ExportSettings {
                    fileConflictResolution = FileConflictResolution.Overwrite
                },
                logger
                );
            export.AddScene(rootObjects, "ExportScene");
            export.SaveToFile(Path.Combine(Application.persistentDataPath,$"ExportScene.gltf"));
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
    }
}
