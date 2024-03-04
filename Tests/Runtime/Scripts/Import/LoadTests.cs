// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GLTFast.Logging;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GLTFast.Tests.Import
{
    [TestFixture]
    class LoadTests
    {
        [GltfTestCase("glTF-test-models", 22)]
        public IEnumerator GltfTestModels(GltfTestCaseSet testCaseSet, GltfTestCase testCase)
        {
            var go = new GameObject();
            var deferAgent = new UninterruptedDeferAgent();
            var loadLogger = new CollectingLogger();
            var path = Path.Combine(testCaseSet.RootPath, testCase.relativeUri);
            Debug.Log($"Loading {testCase} from {path}");

            using (var gltf = new GltfImport(deferAgent: deferAgent, logger: loadLogger))
            {
                var task = gltf.Load(path);
                yield return AsyncWrapper.WaitForTask(task);
                var success = task.Result;
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
                    yield break;
                }
                var instantiateLogger = new CollectingLogger();
                var instantiator = new GameObjectInstantiator(gltf, go.transform, instantiateLogger);
                task = gltf.InstantiateMainSceneAsync(instantiator);
                yield return AsyncWrapper.WaitForTask(task);
                success = task.Result;
                if (!success)
                {
                    instantiateLogger.LogAll();
                    throw new AssertionException("glTF instantiation failed");
                }
                Object.Destroy(go);
                AssertLoggers(new[] { loadLogger, instantiateLogger }, testCase);
            }
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
