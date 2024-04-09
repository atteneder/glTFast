// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if !(UNITY_ANDROID || UNITY_WEBGL) || UNITY_EDITOR
#define LOCAL_LOADING
#endif

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using GLTFast.Export;
using GLTFast.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

using Object = UnityEngine.Object;

#if USING_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
#if GLTF_VALIDATOR
using Unity.glTF.Validator;
#endif // GLTF_VALIDATOR
#endif // UNITY_EDITOR

namespace GLTFast.Tests.Export
{

    [TestFixture, Category("Export")]
    class ExportTests : IPrebuildSetup
    {
        internal const string exportTargetFolder = "gltf-export-targets";

        const string k_SceneNameBuiltIn = "ExportSceneBuiltIn";
        const string k_SceneNameHighDefinition = "ExportSceneHighDefinition";
        const string k_SceneNameUniversal = "ExportSceneUniversal";

        const string k_ScenesPath = "/Tests/Runtime/Export/Scenes/";
#if UNITY_EDITOR
        static void UpdateObjectLists()
        {
            CreateExportSceneObjectList(k_SceneNameBuiltIn);
            CreateExportSceneObjectList(k_SceneNameHighDefinition);
            CreateExportSceneObjectList(k_SceneNameUniversal);
            AssetDatabase.Refresh();
            const string relativePath = "/Tests/Runtime/Scripts/glTFast.Tests.asmdef";
            var asmDefPath = $"Packages/{GltfGlobals.GltfPackageName}{relativePath}";
            TryFixPackageAssetPath(ref asmDefPath);
            AssetDatabase.ImportAsset(asmDefPath, ImportAssetOptions.ForceUpdate);
        }

        internal static void SetupTests()
        {
            CertifyStreamingAssetsFolder();
            AddExportTestScene();
            CopyObjectListsToStreamingAssets();
        }

        internal static void CertifyStreamingAssetsFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets"))
            {
                AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            }
        }

        static void CopyObjectListsToStreamingAssets()
        {
            CopyObjectListToStreamingAssets(k_SceneNameBuiltIn);
            CopyObjectListToStreamingAssets(k_SceneNameHighDefinition);
            CopyObjectListToStreamingAssets(k_SceneNameUniversal);
        }

        /// <summary>
        /// Package tools might split up the tests (`Tests` folder) into a dedicated `.tests` package.
        /// To make searching/loading assets via path easier, this method tries to find the asset in the original
        /// location and if it fails, it searches for it in the tests pacakge and updates/fixes the packageAssetPath.
        /// </summary>
        /// <param name="packageAssetPath">Packages' asset's path</param>
        /// <returns>The GUID of the asset found at that path.</returns>
        /// <exception cref="FileNotFoundException">When the asset was not found.</exception>
        internal static GUID TryFixPackageAssetPath(ref string packageAssetPath)
        {
            Assert.IsTrue(packageAssetPath.StartsWith("Packages/"), $"Not a package asset path {packageAssetPath}");
            var guid = AssetDatabase.GUIDFromAssetPath(packageAssetPath);
            if (guid.Empty())
            {
                packageAssetPath = packageAssetPath.Replace(GltfGlobals.GltfPackageName, $"{GltfGlobals.GltfPackageName}.tests");
                guid = AssetDatabase.GUIDFromAssetPath(packageAssetPath);
                if (guid.Empty())
                {
                    throw new FileNotFoundException($"Couldn't find asset at {packageAssetPath}.");
                }
            }
            return guid;
        }

        internal static void AddExportTestScene()
        {
            var sceneName = GetExportSceneName();
            var scenePath = $"Packages/{GltfGlobals.GltfPackageName}{k_ScenesPath}{sceneName}.unity";
            var sceneGuid = TryFixPackageAssetPath(ref scenePath);

            var scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
            {
                if (scene.guid == sceneGuid)
                {
                    return;
                }
            }

            Array.Resize(ref scenes, scenes.Length+1);
            scenes[scenes.Length - 1] = new EditorBuildSettingsScene(sceneGuid, true);
            EditorBuildSettings.scenes = scenes;

        }

        static void CreateExportSceneObjectList(string sceneName)
        {
            var names = GetRootObjectNamesFromScene(sceneName);
            var assetPath = GetObjectListAssetPath(sceneName);
            var streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, $"{sceneName}.txt");
            File.WriteAllLines(assetPath,names);
            File.WriteAllLines(streamingAssetsPath,names);
        }

        static void CopyObjectListToStreamingAssets(string sceneName)
        {
            var assetPath = GetObjectListAssetPath(sceneName);
            TryFixPackageAssetPath(ref assetPath);
            var streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, $"{sceneName}.txt");
            if (File.Exists(streamingAssetsPath))
            {
                FileUtil.ReplaceFile(assetPath,streamingAssetsPath);
            }
            else
            {
                FileUtil.CopyFileOrDirectory(assetPath,streamingAssetsPath);
            }
        }

        static string GetObjectListAssetPath(string sceneName)
        {
            return $"Packages/{GltfGlobals.GltfPackageName}{k_ScenesPath}{sceneName}-ObjectList.txt";
        }

        static string[] GetRootObjectNamesFromScene(string sceneName)
        {
            var scenePath = $"Packages/{GltfGlobals.GltfPackageName}{k_ScenesPath}{sceneName}.unity";
            TryFixPackageAssetPath(ref scenePath);
            var scene = EditorSceneManager.OpenScene(scenePath);
            var rootObjects = scene.GetRootGameObjects();
            var names = new List<string>();
            for (var i = 0; i < rootObjects.Length; i++) {
                if (rootObjects[i].hideFlags != HideFlags.None) {
                    continue;
                }
                names.Add(rootObjects[i].name);
            }
            return names.ToArray();
        }

        internal static string[] GetRootObjectNamesFromObjectList(string sceneName)
        {
            var assetPath = GetObjectListAssetPath(sceneName);
            TryFixPackageAssetPath(ref assetPath);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            Assert.NotNull(asset, $"glTF Export ObjectList asset at {assetPath} could not be loaded.");
            var objectNames = asset.text.Split('\n');
            return objectNames.Where(name => !string.IsNullOrWhiteSpace(name)).ToArray();
        }
#endif

        internal static string[] GetRootObjectNamesFromStreamingAssets(string sceneName)
        {
            var path = Path.Combine(Application.streamingAssetsPath, $"{sceneName}.txt");
#if LOCAL_LOADING
            path = $"file://{path}";
#endif
            var request = UnityWebRequest.Get(path);
            request.SendWebRequest();
            do { } while (!request.isDone);

            if (request.error != null)
            {
                throw new FileNotFoundException($"ObjectList for scene {sceneName} was not found at {path}: {request.error}");
            }
            var objectNames = request.downloadHandler.text.Split('\n');
            return objectNames.Where(name => !string.IsNullOrWhiteSpace(name)).ToArray();
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SceneManager.LoadScene(GetExportSceneName(), LoadSceneMode.Single);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            SceneManager.UnloadSceneAsync(GetExportSceneName());
        }

        [Test]
        public void CheckObjectCountBuiltIn()
        {
            var names = GetRootObjectNamesFromStreamingAssets(k_SceneNameBuiltIn);
            Assert.AreEqual(48, names.Length, $"Unexpected number of root level objects in scene {k_SceneNameBuiltIn}. If you've added test objects, update the expected number!");
        }

        [Test]
        public void CheckObjectCountUniversal()
        {
            var names = GetRootObjectNamesFromStreamingAssets(k_SceneNameUniversal);
            Assert.AreEqual(48, names.Length, $"Unexpected number of root level objects in scene {k_SceneNameUniversal}. If you've added test objects, update the expected number!");
        }

        [Test]
        public void CheckObjectCountHighDefinition()
        {
            var names = GetRootObjectNamesFromStreamingAssets(k_SceneNameHighDefinition);
            Assert.AreEqual(48, names.Length, $"Unexpected number of root level objects in scene {k_SceneNameHighDefinition}. If you've added test objects, update the expected number!");
        }

        [UnityTest]
        public IEnumerator SimpleTree()
        {
            var root = new GameObject("root");
            var childA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childA.name = "child A";
            var childB = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            childB.name = "child B";
            childA.transform.parent = root.transform;
            childB.transform.parent = root.transform;
            childB.transform.localPosition = new Vector3(1, 0, 0);

            var logger = new CollectingLogger();
            var export = new GameObjectExport(logger: logger);
            export.AddScene(new[] { root }, "UnityScene");
            var path = Path.Combine(Application.persistentDataPath, "root.gltf");
            var task = export.SaveToFileAndDispose(path);
            yield return AsyncWrapper.WaitForTask(task);
            var success = task.Result;
            Assert.IsTrue(success);
            AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(path, MessageCode.UNUSED_OBJECT);
#endif
        }

        [UnityTest, SceneRootObjectTestCase(k_SceneNameBuiltIn, "gltf")]
        public IEnumerator ExportSceneJson(int index, string objectName)
        {
            TryIgnoreDivergentRenderPipeline(RenderPipeline.BuiltIn);
            var gameObject = GetGameObject(index, objectName);
            var task = ExportSceneGameObject(gameObject, binary: false, deterministic: true);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest, SceneRootObjectTestCase(k_SceneNameUniversal, "gltf-urp")]
        public IEnumerator ExportSceneJsonUniversal(int index, string objectName)
        {
            TryIgnoreDivergentRenderPipeline(RenderPipeline.Universal);
            var gameObject = GetGameObject(index, objectName);
            var task = ExportSceneGameObject(gameObject, binary: false, deterministic: true);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest, SceneRootObjectTestCase(k_SceneNameHighDefinition, "gltf-hdrp")]
        public IEnumerator ExportSceneJsonHighDefinition(int index, string objectName)
        {
            TryIgnoreDivergentRenderPipeline(RenderPipeline.HighDefinition);
            var gameObject = GetGameObject(index, objectName);
            var task = ExportSceneGameObject(gameObject, binary: false, deterministic: true);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest, SceneRootObjectTestCase(k_SceneNameBuiltIn, "glb")]
        public IEnumerator ExportSceneBinary(int index, string objectName)
        {
            TryIgnoreDivergentRenderPipeline(RenderPipeline.BuiltIn);
            var gameObject = GetGameObject(index, objectName);
            var task = ExportSceneGameObject(gameObject, true);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest, SceneRootObjectTestCase(k_SceneNameUniversal, "glb-urp")]
        public IEnumerator ExportSceneBinaryUniversal(int index, string objectName)
        {
            TryIgnoreDivergentRenderPipeline(RenderPipeline.Universal);
            var gameObject = GetGameObject(index, objectName);
            var task = ExportSceneGameObject(gameObject, true);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest, SceneRootObjectTestCase(k_SceneNameHighDefinition, "glb-hdrp")]
        public IEnumerator ExportSceneBinaryHighDefinition(int index, string objectName)
        {
            TryIgnoreDivergentRenderPipeline(RenderPipeline.HighDefinition);
            var gameObject = GetGameObject(index, objectName);
            var task = ExportSceneGameObject(gameObject, true);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest]
        public IEnumerator ExportSceneAllJson()
        {
            yield return null;
            var task = ExportSceneAll(false);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest]
        public IEnumerator ExportSceneAllBinary()
        {
            yield return null;
            var task = ExportSceneAll(true);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest, SceneRootObjectTestCase(k_SceneNameBuiltIn, "stream")]
        public IEnumerator ExportSceneBinaryStream(int index, string objectName)
        {
            TryIgnoreDivergentRenderPipeline(RenderPipeline.BuiltIn);
            var gameObject = GetGameObject(index, objectName);
            var task = ExportSceneGameObject(gameObject, true, true);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest, SceneRootObjectTestCase(k_SceneNameUniversal, "stream-urp")]
        public IEnumerator ExportSceneBinaryStreamUniversal(int index, string objectName)
        {
            TryIgnoreDivergentRenderPipeline(RenderPipeline.Universal);
            var gameObject = GetGameObject(index, objectName);
            var task = ExportSceneGameObject(gameObject, true, true);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest, SceneRootObjectTestCase(k_SceneNameHighDefinition, "stream-hdrp")]
        public IEnumerator ExportSceneBinaryStreamHighDefinition(int index, string objectName)
        {
            TryIgnoreDivergentRenderPipeline(RenderPipeline.HighDefinition);
            var gameObject = GetGameObject(index, objectName);
            var task = ExportSceneGameObject(gameObject, true, true);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest]
        public IEnumerator ExportSceneAllBinaryStream()
        {
            yield return null;
            var task = ExportSceneAll(true, true);
            yield return AsyncWrapper.WaitForTask(task);
        }

        [Test]
        public void MeshMaterialCombinationTest()
        {

            var mc1 = new MeshMaterialCombination(42, new[] { 1, 2, 3 });
            var mc2 = new MeshMaterialCombination(42, new[] { 1, 2, 3 });

            Assert.AreEqual(mc1, mc2);

            mc1 = new MeshMaterialCombination(42, new[] { 1, 2, 4 });
            Assert.AreNotEqual(mc1, mc2);

            mc1 = new MeshMaterialCombination(42, new[] { 1, 2 });
            Assert.AreNotEqual(mc1, mc2);

            mc1 = new MeshMaterialCombination(42, null);
            Assert.AreNotEqual(mc1, mc2);

            mc2 = new MeshMaterialCombination(42, null);
            Assert.AreEqual(mc1, mc2);

            mc1 = new MeshMaterialCombination(13, null);
            Assert.AreNotEqual(mc1, mc2);
        }

        [UnityTest]
        public IEnumerator TwoScenes()
        {

            var childA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childA.name = "child A";

            var logger = new CollectingLogger();
            var export = new GameObjectExport(logger: logger);
            export.AddScene(new[] { childA }, "scene A");
            export.AddScene(new[] { childA }, "scene B");
            var path = Path.Combine(Application.persistentDataPath, "TwoScenes.gltf");
            var task = export.SaveToFileAndDispose(path);
            yield return AsyncWrapper.WaitForTask(task);
            var success = task.Result;
            Assert.IsTrue(success);
            AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(path, MessageCode.UNUSED_OBJECT);
#endif
        }

        [UnityTest]
        public IEnumerator Empty()
        {

            var logger = new CollectingLogger();
            var export = new GameObjectExport(logger: logger);
            var path = Path.Combine(Application.persistentDataPath, "Empty.gltf");
            var task = export.SaveToFileAndDispose(path);
            yield return AsyncWrapper.WaitForTask(task);
            var success = task.Result;
            Assert.IsTrue(success);
            AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(path, MessageCode.UNUSED_OBJECT);
#endif
        }

        [UnityTest]
        public IEnumerator ComponentMask()
        {

            var root = new GameObject("Root");

            var meshGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshGo.name = "Mesh";
            meshGo.transform.SetParent(root.transform);
            meshGo.transform.localPosition = new Vector3(0, 0, 0);

            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(root.transform);
            lightGo.transform.localPosition = new Vector3(1, 0, 0);
            lightGo.AddComponent<Light>();
#if USING_HDRP
            lightGo.AddComponent<HDAdditionalLightData>();
#endif
            var cameraGo = new GameObject("Camera");
            cameraGo.transform.SetParent(root.transform);
            cameraGo.transform.localPosition = new Vector3(.5f, 0, -3);
            cameraGo.AddComponent<Camera>();

            // Export no components
            var task = ExportTest(
                new[] { root },
                "ComponentMaskNone",
                new ExportSettings
                {
                    ComponentMask = ComponentType.None
                });
            yield return AsyncWrapper.WaitForTask(task);

            // Export mesh only
            task = ExportTest(
                new[] { root },
                "ComponentMaskMesh",
                new ExportSettings
                {
                    ComponentMask = ComponentType.Mesh
                });
            yield return AsyncWrapper.WaitForTask(task);

            // Export light only
            task = ExportTest(
                new[] { root },
                "ComponentMaskLight",
                new ExportSettings
                {
                    ComponentMask = ComponentType.Light
                });
            yield return AsyncWrapper.WaitForTask(task);

            // Export Camera only
            task = ExportTest(
                new[] { root },
                "ComponentMaskCamera",
                new ExportSettings
                {
                    ComponentMask = ComponentType.Camera
                });
            yield return AsyncWrapper.WaitForTask(task);

            // Clean up
            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator LayerMask()
        {

            var root = new GameObject("Root");

            var childA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childA.name = "a";
            childA.transform.SetParent(root.transform);
            childA.transform.localPosition = new Vector3(0, 0, 0);

            var childB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childB.name = "b";
            childB.transform.SetParent(childA.transform);
            childB.transform.localPosition = new Vector3(1, 0, 0);

            var childC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childC.name = "c";
            childC.transform.SetParent(childB.transform);
            childC.transform.localPosition = new Vector3(1, 0, 0);

            childA.layer = 1; // On layer 0
            childB.layer = 1; // On layer 0
            childC.layer = 2; // On layer 1

            // Export all layers
            var task = ExportTest(
                new[] { root },
                "LayerMaskAll",
                gameObjectExportSettings: new GameObjectExportSettings
                {
                    LayerMask = ~0
                });
            yield return AsyncWrapper.WaitForTask(task);

            // Export layer 1
            task = ExportTest(
                new[] { root },
                "LayerMaskOne",
                gameObjectExportSettings: new GameObjectExportSettings
                {
                    LayerMask = 1
                });
            yield return AsyncWrapper.WaitForTask(task);

            // Export layer 2
            task = ExportTest(
                new[] { root },
                "LayerMaskTwo",
                gameObjectExportSettings: new GameObjectExportSettings
                {
                    LayerMask = 2
                });
            yield return AsyncWrapper.WaitForTask(task);

            // Export no layer
            task = ExportTest(
                new[] { root },
                "LayerMaskNone",
                gameObjectExportSettings: new GameObjectExportSettings
                {
                    LayerMask = 0
                });
            yield return AsyncWrapper.WaitForTask(task);

            // Clean up
            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator SavedTwice()
        {

            var childA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childA.name = "child A";

            var logger = new CollectingLogger();
            var export = new GameObjectExport(logger: logger);
            export.AddScene(new[] { childA });
            var path = Path.Combine(Application.persistentDataPath, "SavedTwice1.gltf");
            var task = export.SaveToFileAndDispose(path);
            yield return AsyncWrapper.WaitForTask(task);
            var success = task.Result;
            Assert.IsTrue(success);
            AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(path, MessageCode.UNUSED_OBJECT);
#endif
            Assert.Throws<InvalidOperationException>(delegate
            {
                export.AddScene(new[] { childA });
            });
            path = Path.Combine(Application.persistentDataPath, "SavedTwice2.gltf");
            AssertThrowsAsync<InvalidOperationException>(async () => await export.SaveToFileAndDispose(path));
        }

        static async Task ExportTest(
            GameObject[] objects,
            string testName,
            ExportSettings exportSettings = null,
            GameObjectExportSettings gameObjectExportSettings = null
            )
        {
            var logger = new CollectingLogger();
            var export = new GameObjectExport(
                exportSettings: exportSettings,
                gameObjectExportSettings: gameObjectExportSettings,
                logger: logger
                );
            export.AddScene(objects);
            var resultPath = Path.Combine(Application.persistentDataPath, $"{testName}.gltf");
            var success = await export.SaveToFileAndDispose(resultPath);
            Assert.IsTrue(success);
            AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(resultPath, MessageCode.UNUSED_OBJECT);
#endif
            // #if UNITY_EDITOR
            //             AssertGltfJson($"{testName}.gltf", resultPath);
            // #endif
        }

        static GameObject GetGameObject(int index, string objectName)
        {
            var scene = SceneManager.GetActiveScene();
            var objects = scene.GetRootGameObjects();
            var gameObject = objects[index];
            if (gameObject.name != objectName)
            {
                // GameObject order is not deterministic in builds, so here we
                // search by traversing all root objects.
                foreach (var obj in objects)
                {
                    if (obj.name == objectName)
                    {
                        gameObject = obj;
                        break;
                    }
                }
            }

            Assert.NotNull(gameObject);
            Assert.AreEqual(objectName, gameObject.name);
            return gameObject;
        }

        static async Task ExportSceneGameObject(GameObject gameObject, bool binary, bool toStream = false, bool deterministic = false)
        {
            var logger = new CollectingLogger();
            var export = new GameObjectExport(
                new ExportSettings
                {
                    Format = binary ? GltfFormat.Binary : GltfFormat.Json,
                    FileConflictResolution = FileConflictResolution.Overwrite,
                    Deterministic = deterministic,
                },
                logger: logger
            );
            export.AddScene(new[] { gameObject }, gameObject.name);
            var extension = binary ? GltfGlobals.GlbExt : GltfGlobals.GltfExt;
            var fileName = $"{gameObject.name}{extension}";
            var path = Path.Combine(Application.persistentDataPath, fileName);

            bool success;
            if (toStream)
            {
                var glbStream = new MemoryStream();
                success = await export.SaveToStreamAndDispose(glbStream);
                Assert.Greater(glbStream.Length, 20);
                glbStream.Close();
            }
            else
            {
                success = await export.SaveToFileAndDispose(path);
            }
            Assert.IsTrue(success);
            AssertLogger(logger);

            if (!binary)
            {
#if UNITY_EDITOR
                AssertGltfJson(fileName, path);
#else
                await AssertGltfJson(fileName, path);
#endif
            }

#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(path, new [] {
                MessageCode.ACCESSOR_MAX_MISMATCH,
                MessageCode.ACCESSOR_MIN_MISMATCH,
                MessageCode.NODE_EMPTY,
                MessageCode.UNUSED_OBJECT,
            });
#endif
        }

#if UNITY_EDITOR
        static void AssertGltfJson(string testName, string resultPath)
#else
        static async Task AssertGltfJson(string testName, string resultPath)
#endif
        {

            var renderPipeline = RenderPipelineUtils.RenderPipeline;
            string rpSubfolder;
            switch (renderPipeline)
            {
                case RenderPipeline.Universal:
                    rpSubfolder = "/URP";
                    break;
                case RenderPipeline.HighDefinition:
                    rpSubfolder = "/HDRP";
                    break;
                default:
                    rpSubfolder = "";
                    break;
            }

#if UNITY_EDITOR
            var pathPrefix = $"Packages/{GltfGlobals.GltfPackageName}/Tests/Resources/ExportTargets";
            TryFixPackageAssetPath(ref pathPrefix);

            // TODO: Load via File.ReadAllLinesAsync once 2020 LTS support is dropped
            //       to removed async/non-async dichotomy
            var assetPath = $"{pathPrefix}{rpSubfolder}/{testName}.txt";
            var targetJsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (targetJsonAsset == null) {
                assetPath = $"{pathPrefix}/{testName}.txt";
                targetJsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            }
            var targetJson = targetJsonAsset.text;
#else
            var path = Path.Combine(Application.streamingAssetsPath, $"{exportTargetFolder}{rpSubfolder}/{testName}.txt");
#if LOCAL_LOADING
            path = $"file://{path}";
#endif
            var request = UnityWebRequest.Get(path);

            var x = request.SendWebRequest();
            while (!x.isDone)
            {
                await Task.Yield();
            }

            Assert.IsNull(request.error, $"Target glTF JSON for {testName} was not found at {path}: {request.error}");

            var targetJson = request.downloadHandler.text;
#endif

            CompareGltfJsonTokenRecursively(
                JToken.Parse(targetJson),
                JToken.Parse(File.ReadAllText(resultPath))
                );
        }

        static void CompareGltfJsonTokenRecursively(JToken tokenA, JToken tokenB)
        {

            foreach (var (a, b) in tokenA.Zip(tokenB, Tuple.Create))
            {
                Assert.AreEqual(a.Path, b.Path, $"Path mismatch ({a.Path} != {b.Path}");
                // Assert.AreEqual(a.Type,b.Type, $"Type mismatch at {a.Path} ({a.Type} != {b.Type}");
                if (a.Type != b.Type)
                {
                    if (
                        (a.Type == JTokenType.Float && b.Type != JTokenType.Integer)
                        || (a.Type == JTokenType.Integer && b.Type != JTokenType.Float)
                    )
                    {
                        throw new AssertionException($"Type mismatch at {a.Path} ({a.Type} != {b.Type}");
                    }
                }
                if (a is JValue && b is JValue)
                {
                    switch (a.Type)
                    {
                        case JTokenType.Float:
                        case JTokenType.Integer:
                            var expected = a.Value<double>();
                            var actual = b.Value<double>();
                            Assert.That(actual, Is.EqualTo(expected).Within(6E-08f), $"Value mismatch at {a.Path}.");
                            break;
                        case JTokenType.String:
                            Assert.AreEqual(a.Value<string>(), b.Value<string>(), $"Value mismatch at {a.Path}.");
                            break;
                        case JTokenType.Boolean:
                            Assert.AreEqual(a.Value<bool>(), b.Value<bool>(), $"Value mismatch at {a.Path}.");
                            break;
                        default:
                            Assert.AreEqual(a, b, $"Value mismatch at {a.Path}.");
                            break;
                    }
                }

                // asset.generator usually contains differing Unity and glTFast versions, so we ignore its value
                if (a.Path == "asset.generator") continue;

                CompareGltfJsonTokenRecursively(a, b);
            }
        }

        static async Task ExportSceneAll(bool binary, bool toStream = false)
        {
            var sceneName = GetExportSceneName();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

            var scene = SceneManager.GetActiveScene();

            var rootObjects = scene.GetRootGameObjects();

            var logger = new CollectingLogger();
            var export = new GameObjectExport(
                new ExportSettings
                {
                    Format = binary ? GltfFormat.Binary : GltfFormat.Json,
                    FileConflictResolution = FileConflictResolution.Overwrite,
                },
                logger: logger
            );
            export.AddScene(rootObjects, sceneName);
            var extension = binary ? GltfGlobals.GlbExt : GltfGlobals.GltfExt;
            var path = Path.Combine(Application.persistentDataPath, $"ExportScene{extension}");

            bool success;
            if (toStream)
            {
                var glbStream = new MemoryStream();
                success = await export.SaveToStreamAndDispose(glbStream);
                Assert.Greater(glbStream.Length, 20);
                glbStream.Close();
            }
            else
            {
                success = await export.SaveToFileAndDispose(path);
            }
            Assert.IsTrue(success);
            AssertLogger(logger);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ValidateGltf(path, new [] {
                MessageCode.ACCESSOR_ELEMENT_OUT_OF_MAX_BOUND,
                MessageCode.ACCESSOR_MAX_MISMATCH,
                MessageCode.ACCESSOR_MIN_MISMATCH,
                MessageCode.NODE_EMPTY,
                MessageCode.UNUSED_OBJECT,
            });
#endif
        }

        static void AssertLogger(CollectingLogger logger)
        {
            logger.LogAll();
            if (logger.Count > 0)
            {
                foreach (var item in logger.Items)
                {
#if !UNITY_IMAGECONVERSION
                    if (item.Type == LogType.Warning && item.Code == LogCode.ImageConversionNotEnabled)
                    {
                        continue;
                    }
#endif
                    Assert.AreEqual(LogType.Log, item.Type, item.ToString());
                }
            }
        }

        /// <summary>
        /// Fill-in for NUnit's Assert.ThrowsAsync
        /// Source: https://forum.unity.com/threads/can-i-replace-upgrade-unitys-nunit.488580/#post-6543523
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <typeparam name="TActual"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        static void AssertThrowsAsync<TActual>(AsyncTestDelegate code, string message = "", params object[] args) where TActual : Exception
        {
            Assert.Throws<TActual>(() =>
            {
                try
                {
                    code.Invoke().Wait(); // Will wrap any exceptions in an AggregateException
                }
                catch (AggregateException e)
                {
                    if (e.InnerException is null)
                    {
                        throw;
                    }
                    throw e.InnerException; // Throw the unwrapped exception
                }
            }, message, args);
        }

        delegate Task AsyncTestDelegate();


#if GLTF_VALIDATOR && UNITY_EDITOR
        static void ValidateGltf(string path, params MessageCode[] expectedMessages) {
            var report = Validator.Validate(path);
            Assert.NotNull(report, $"Report null for {path}");
            // report.Log();
            if (report.issues != null) {
                foreach (var message in report.issues.messages) {
                    if (((IList)expectedMessages).Contains(message.codeEnum)) {
                        continue;
                    }
                    Assert.Greater(message.severity, 0, $"Error {message} (path {Path.GetFileName(path)})");
                    Assert.Greater(message.severity, 1, $"Warning {message} (path {Path.GetFileName(path)})");
                }
            }
        }
#endif

        static string GetExportSceneName()
        {
            switch (RenderPipelineUtils.RenderPipeline)
            {
                case RenderPipeline.HighDefinition:
                    return k_SceneNameHighDefinition;
                case RenderPipeline.Universal:
                    return k_SceneNameUniversal;
                default:
                    return k_SceneNameBuiltIn;
            }
        }

        protected static void TryIgnoreDivergentRenderPipeline(RenderPipeline expectedPipeline)
        {
            var actualPipeline = RenderPipelineUtils.RenderPipeline;
            if (expectedPipeline != actualPipeline)
            {
                Assert.Ignore($"Ignore {expectedPipeline} test in {actualPipeline} setup.");
            }
        }

        public void Setup()
        {
#if UNITY_EDITOR
            SetupTests();
#endif
        }
    }
}
