// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.IO;
using System.Threading.Tasks;
using GLTFast.Export;
using GLTFast.Logging;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GLTFast.Tests.Export
{
    class GltfWriterTests
    {
        [UnityTest]
        public IEnumerator ToStreamNotSelfContained()
        {
            var logger = new CollectingLogger();
            var writer = new GltfWriter(
                new ExportSettings
                {
                    Format = GltfFormat.Binary,
                    ImageDestination = ImageDestination.SeparateFile
                },
                logger: logger
            );

            yield return AsyncWrapper.WaitForTask(
                writer.SaveToStreamAndDispose(new MemoryStream())
            );
            LoggerTest.AssertLogger(
                logger,
                new[]
                {
                    new LogItem(
                        LogType.Error,
                        LogCode.None,
                        "Save to Stream currently only works for self-contained glTF-Binary"
                        )
                });
        }

        [UnityTest]
        public IEnumerator DracoUncompressedFallback()
        {
#if DRACO_UNITY
            var logger = new CollectingLogger();
            yield return AsyncWrapper.WaitForTask(
                DracoUncompressedFallback(logger)
                );

            LoggerTest.AssertLogger(
                logger,
                new[]
                {
                    new LogItem(
                        LogType.Warning,
                        LogCode.UncompressedFallbackNotSupported
                    )
                });
#else
            Assert.Ignore("Requires Draco for Unity package to be installed.");
            yield return null;
#endif
        }

        [UnityTest]
        public IEnumerator DracoUncompressedFallbackNoLogger()
        {
#if DRACO_UNITY
            yield return AsyncWrapper.WaitForTask(
                DracoUncompressedFallback(null)
            );
#else
            Assert.Ignore("Requires Draco for Unity package to be installed.");
            yield return null;
#endif
        }

        static async Task DracoUncompressedFallback(ICodeLogger logger)
        {
            var writer = new GltfWriter(
                new ExportSettings
                {
                    Format = GltfFormat.Binary,
                    Compression = Compression.Uncompressed | Compression.Draco
                },
                logger: logger
            );

            var node = writer.AddNode();
            var tmpGameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            writer.AddMeshToNode((int)node, tmpGameObject.GetComponent<MeshFilter>().sharedMesh, null);

            await writer.SaveToStreamAndDispose(new MemoryStream());

            Object.Destroy(tmpGameObject);
        }
    }
}
