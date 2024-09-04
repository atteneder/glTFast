// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.IO;
using System.Threading.Tasks;
using GLTFast.Export;
using GLTFast.Logging;
using NUnit.Framework;
#if GLTF_VALIDATOR && UNITY_EDITOR
using Unity.glTF.Validator;
#endif // GLTF_VALIDATOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GLTFast.Tests.Export
{
    [TestFixture, Category("Export")]
    class ExportSkinTests : IPrebuildSetup
    {
        const string k_SceneName = "ExportSkin";

#if UNITY_EDITOR
        static void SetupTests()
        {
            ExportTests.AddExportTestScene(k_SceneName);
        }
#endif

        public void Setup()
        {
#if UNITY_EDITOR
            SetupTests();
#endif
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SceneManager.LoadScene(k_SceneName, LoadSceneMode.Single);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            SceneManager.UnloadSceneAsync(k_SceneName);
        }

        [UnityTest]
        public IEnumerator SimpleSkin()
        {
            var skin = GameObject.Find("SimpleSkin");
            var task = Export("SimpleSkin", new[] { skin });
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest]
        public IEnumerator SimpleSkinTwice()
        {
            var skin = GameObject.Find("SimpleSkin");
            var skin2 = GameObject.Find("SimpleSkin2");
            var task = Export("SimpleSkinTwice", new[] { skin, skin2 });
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest]
        public IEnumerator SimpleSkinDraco()
        {
#if !DRACO_UNITY
            Assert.Ignore("Test requires Draco for Unity (com.unity.cloud.draco) to be installed");
#endif
            var skin = GameObject.Find("SimpleSkin");
            var task = Export(
                "SimpleSkinDraco",
                new[] { skin },
                new ExportSettings
                {
                    Compression = Compression.Draco
                });
            yield return AsyncWrapper.WaitForTask(task);
        }

        [UnityTest]
        public IEnumerator MissingBones()
        {
            LogAssert.Expect(LogType.Error, "Skip skin on Node-0: No node ID for bone transform Node-1 found!");

            var skin = GameObject.Find("SimpleSkin").transform;
            var meshOnly = skin.GetChild(0).gameObject;
            var task = Export("MissingBones", new[] { meshOnly });
            yield return AsyncWrapper.WaitForTask(task);
        }

        static async Task Export(string name, GameObject[] objects, ExportSettings settings = null)
        {
            var logger = new CollectingLogger();
            var export = new GameObjectExport(logger: logger, exportSettings: settings);
            export.AddScene(objects);
            var path = Path.Combine(Application.persistentDataPath, $"{name}.gltf");
            var success = await export.SaveToFileAndDispose(path); ;
            ExportTests.AssertLogger(logger);
            Assert.IsTrue(success);
#if GLTF_VALIDATOR && UNITY_EDITOR
            ExportTests.ValidateGltf(path,MessageCode.NODE_SKINNED_MESH_NON_ROOT);
#endif
        }
    }
}
