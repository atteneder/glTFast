// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using GLTFast.Vertex;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GLTFast.Tests
{
    class VertexBufferTexCoordsTests
    {
        [Test]
        public void SparseTexCoords()
        {
            var v = new VertexBufferTexCoords<VTexCoord1>(null);

            var handles = new NativeArray<JobHandle>(1, Allocator.Temp);
            var success = v.ScheduleVertexUVJobs(
                new GltfBufferMock(),
                new[] { GltfBufferMock.sparseAccessorIndex },
                3,
                handles);
            Assert.IsFalse(success);
            v.Dispose();
            handles.Dispose();
        }
    }
}
