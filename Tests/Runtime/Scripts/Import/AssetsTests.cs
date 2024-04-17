// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast.Logging;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GLTFast.Tests.Import
{
    [TestFixture]
    class AssetsTests
    {
        [GltfTestCase("glTF-test-models", 22)]
        public IEnumerator GltfTestModels(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
            yield return AsyncWrapper.WaitForTask(RunTestCase(testCaseSet, testCase));
        }

        [GltfTestCase("glTF-Sample-Assets", 101, @"glTF(-JPG-PNG)?\/.*\.gltf$")]
        public IEnumerator KhronosGltfSampleAssets(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
            yield return AsyncWrapper.WaitForTask(RunTestCase(testCaseSet, testCase));
        }

        [GltfTestCase("glTF-Sample-Assets", 7, @"glTF-Binary\/.*\.glb$")]
        public IEnumerator KhronosGltfSampleAssetsBinary(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
            yield return AsyncWrapper.WaitForTask(RunTestCase(testCaseSet, testCase));
        }

        [GltfTestCase("glTF-Sample-Assets", 4, @"glTF-Draco\/.*\.gltf$")]
        public IEnumerator KhronosGltfSampleAssetsDraco(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
#if DRACO_UNITY
            yield return AsyncWrapper.WaitForTask(RunTestCase(testCaseSet, testCase));
#else
            Assert.Ignore("Requires Draco for Unity package to be installed.");
            yield break;
#endif
        }

        [GltfTestCase("glTF-Sample-Assets", 2, @"glTF-KTX-BasisU\/.*\.gltf$")]
        public IEnumerator KhronosGltfSampleAssetsKtx(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
#if KTX_UNITY
            yield return AsyncWrapper.WaitForTask(RunTestCase(testCaseSet, testCase));
#else
            Assert.Ignore("Requires KTX for Unity package to be installed.");
            yield break;
#endif
        }

        [GltfTestCase("glTF-Sample-Assets", 1, @"StainedGlassLamp\/glTF-KTX-BasisU\/StainedGlassLamp.gltf$")]
        public IEnumerator KtxMissing(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
#if KTX_UNITY
            yield return null;
            Assert.Ignore("Requires absence of KTX for Unity package.");
#else
            testCase = new GltfTestCase
            {
                relativeUri = testCase.relativeUri,
                expectLoadFail = true,
                expectInstantiationFail = testCase.expectInstantiationFail,
                expectedLogCodes = new[] { LogCode.PackageMissing }
            };
            yield return AsyncWrapper.WaitForTask(RunTestCase(testCaseSet, testCase));
#endif
        }

        [GltfTestCase("glTF-Sample-Assets", 1, @"Box\/glTF-Draco\/Box.gltf$")]
        public IEnumerator DracoMissing(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
#if DRACO_UNITY
            yield return null;
            Assert.Ignore("Requires absence of Draco for Unity package.");
#else
            testCase = new GltfTestCase
            {
                relativeUri = testCase.relativeUri,
                expectLoadFail = true,
                expectInstantiationFail = testCase.expectInstantiationFail,
                expectedLogCodes = new[] { LogCode.PackageMissing }
            };
            yield return AsyncWrapper.WaitForTask(RunTestCase(testCaseSet, testCase));
#endif
        }

        [GltfTestCase("glTF-Sample-Assets", 8, @"glTF-Embedded\/.*\.gltf$")]
        public IEnumerator KhronosGltfSampleAssetsEmbedded(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
            yield return AsyncWrapper.WaitForTask(RunTestCase(testCaseSet, testCase));
        }

        [GltfTestCase("glTF-Sample-Assets", 1, @"glTF-Quantized\/.*\.gltf$")]
        public IEnumerator KhronosGltfSampleAssetsQuantized(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
            yield return AsyncWrapper.WaitForTask(RunTestCase(testCaseSet, testCase));
        }

        static async Task RunTestCase(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
            var go = new GameObject();
            var deferAgent = new UninterruptedDeferAgent();
            var loadLogger = new CollectingLogger();
            var path = Path.Combine(testCaseSet.RootPath, testCase.relativeUri);
            Debug.Log($"Loading {testCase} from {path}");

            using var gltf = new GltfImport(deferAgent: deferAgent, logger: loadLogger);
            var success = await gltf.Load(path);
            if (success ^ !testCase.expectLoadFail)
            {
                AssertLoggers(new[] { loadLogger }, testCase);
                if (success)
                {
                    throw new AssertionException("glTF import unexpectedly succeeded!");
                }

                throw new AssertionException("glTF import failed!");
            }

            if (!success)
            {
                AssertLoggers(new[] { loadLogger }, testCase);
                return;
            }
            var instantiateLogger = new CollectingLogger();
            var instantiator = new GameObjectInstantiator(gltf, go.transform, instantiateLogger);
            success = await gltf.InstantiateMainSceneAsync(instantiator);
            if (!success)
            {
                instantiateLogger.LogAll();
                throw new AssertionException("glTF instantiation failed");
            }
            Object.Destroy(go);
            AssertLoggers(new[] { loadLogger, instantiateLogger }, testCase);
        }

        static void AssertLoggers(IEnumerable<CollectingLogger> loggers, GltfTestCase testCase)
        {
            AssertLogItems(IterateLoggerItems(), testCase);
            return;

            IEnumerable<LogItem> IterateLoggerItems()
            {
                foreach (var logger in loggers)
                {
                    if (logger.Count < 1) continue;
                    foreach (var item in logger.Items)
                    {
                        yield return item;
                    }
                }
            }
        }

        internal static void AssertLogItems(IEnumerable<LogItem> logItems, GltfTestCase testCase)
        {
            LoggerTest.AssertLogCodes(logItems, testCase.expectedLogCodes);
        }
    }
}
