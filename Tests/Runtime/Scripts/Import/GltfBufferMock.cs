// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Schema;

namespace GLTFast.Tests
{
    unsafe class GltfBufferMock : IGltfBuffers
    {
        public const int sparseAccessorIndex = 42;

        public void GetAccessor(int index, out AccessorBase accessor, out void* data, out int byteStride)
        {
            accessor = new Accessor
            {
                bufferView = index,
                componentType = GltfComponentType.Float,
                sparse = index == sparseAccessorIndex
                    ? new AccessorSparse()
                    : null,
                min = new float[] { -1, -1, -1 },
                max = new float[] { 1, 1, 1 }
            };
            accessor.SetAttributeType(GltfAccessorAttributeType.VEC3);
            data = null;
            byteStride = 1;
        }

        public void GetAccessorSparseIndices(AccessorSparseIndices sparseIndices, out void* data)
        {
            data = null;
        }

        public void GetAccessorSparseValues(AccessorSparseValues sparseValues, out void* data)
        {
            data = null;
        }
    }
}
