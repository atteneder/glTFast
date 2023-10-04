// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace GLTFast
{

    using Schema;

    interface IGltfBuffers
    {
        unsafe void GetAccessor(int index, out AccessorBase accessor, out void* data, out int byteStride);
        unsafe void GetAccessorSparseIndices(AccessorSparseIndices sparseIndices, out void* data);
        unsafe void GetAccessorSparseValues(AccessorSparseValues sparseValues, out void* data);
    }
}
