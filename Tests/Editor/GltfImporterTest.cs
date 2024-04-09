// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using GLTFast.Tests.Import;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

#if !GLTFAST_EDITOR_IMPORT_OFF
using System.IO;
using UnityEngine.TestTools;
#endif

namespace GLTFast.Editor.Tests
{
    [TestFixture]
    class GltfImporterTest
    {
        const string k_TestPath = "Temp-glTF-tests";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            if (!AssetDatabase.IsValidFolder($"Assets/{k_TestPath}"))
            {
                AssetDatabase.CreateFolder("Assets", k_TestPath);
                AssetDatabase.Refresh();
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AssetDatabase.DeleteAsset($"Assets/{k_TestPath}");
            AssetDatabase.Refresh();
        }

        [GltfTestCase("glTF-test-models", 22)]
        public IEnumerator GltfTestModels(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
#if GLTFAST_EDITOR_IMPORT_OFF
            Assert.Ignore("glTF Editor import is disabled via GLTFAST_EDITOR_IMPORT_OFF scripting define.");
#else
            var directories = testCase.relativeUri.Split('/');
            Assert.NotNull(directories);
            Assert.GreaterOrEqual(directories.Length, 2);
            var assetDir = directories[0];
            var destination = $"Assets/{k_TestPath}/{assetDir}";
            var assetPath = Path.Combine("Assets", k_TestPath, testCase.relativeUri);

            if (testCase.expectedLogCodes.Length > 0)
            {
                LogAssert.Expect(LogType.Error, $"Failed to import {assetPath.Replace('\\', '/')} (see inspector for details)");
            }

            if (!AssetDatabase.IsValidFolder(destination))
            {
                var sourcePath = Path.Combine(testCaseSet.RootPath, assetDir);
                FileUtil.CopyFileOrDirectory(sourcePath, destination);
                AssetDatabase.Refresh();
            }

            var importer = (GltfImporter)AssetImporter.GetAtPath(assetPath);
            Assert.NotNull(importer, $"No glTF importer at {assetPath}");

            LoadTests.AssertLogItems(importer.reportItems, testCase);
#endif
            yield return null;
        }
    }
}
