// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.IO;
using System.Threading.Tasks;
using GLTFast.Export;
using GLTFast.Logging;
using GLTFast.Schema;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace GLTFast.Tests.Export
{
    class SceneOriginTests
    {
        static Vector3EqualityComparer s_Vector3Comparer;
        static QuaternionEqualityComparer s_QuaternionComparer;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            s_Vector3Comparer = new Vector3EqualityComparer(10e-6f);
            s_QuaternionComparer = new QuaternionEqualityComparer(10e-6f);
        }

        [UnityTest]
        public IEnumerator SceneOrigin()
        {
            var task = ExportScene(
                new float3(42, 0, 0),
                new float3(0, 13, 0)
            );
            yield return AsyncWrapper.WaitForTask(task);
            var node = task.Result;
            node.GetTransform(out var position, out _, out _);
            Assert.That(position, Is.EqualTo(new Vector3(42, 13, 0)).Using(s_Vector3Comparer));
        }

        [UnityTest]
        public IEnumerator SceneOriginRotated()
        {
            var task = ExportScene(
                new float3(42, 0, 0),
                new float3(0, 13, 0),
                math.inverse(float4x4.TRS(
                    new float3(0, 0, 7),
                    quaternion.Euler(0, math.PIHALF, 0),
                    new float3(.1f))
                    )
            );
            yield return AsyncWrapper.WaitForTask(task);
            var node = task.Result;
            node.GetTransform(out var position, out var rotation, out var scale);
            Assert.That(position, Is.EqualTo(new Vector3(70, 130, 420)).Using(s_Vector3Comparer));
            Assert.That(
                rotation,
                Is.EqualTo(Quaternion.Euler(0, -90, 0))
                    .Using(s_QuaternionComparer)
            );
            Assert.That(
                scale,
                Is.EqualTo(new Vector3(10, 10, 10)).Using(s_Vector3Comparer));
        }

        static async Task<NodeBase> ExportScene(
            float3 parentPosition,
            float3 nodePosition,
            float4x4? sceneOrigin = null
            )
        {
            var root = new GameObject("root");
            root.transform.position = parentPosition;

            var node = new GameObject("node");
            node.transform.SetParent(root.transform, false);
            node.transform.localPosition = nodePosition;

            var logger = new CollectingLogger();
            var export = new GameObjectExport(logger: logger);
            export.AddScene(new[] { node }, sceneOrigin ?? float4x4.identity, "UnityScene");

            var path = Path.Combine(Application.persistentDataPath, "SceneOrigin.gltf");
            var success = await export.SaveToFileAndDispose(path);
            Assert.IsTrue(success);
            LoggerTest.AssertLogger(logger);

            var jsonParser = new GltfJsonUtilityParser();
            var gltf = jsonParser.ParseJson(
#if UNITY_2021_3_OR_NEWER
                await File.ReadAllTextAsync(path)
#else
                File.ReadAllText(path)
#endif
                );

            Assert.IsNotNull(gltf?.Nodes);
            Assert.AreEqual(1, gltf.Nodes.Count);
            return gltf.Nodes[0];
        }
    }
}
