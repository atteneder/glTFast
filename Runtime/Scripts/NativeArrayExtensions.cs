// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Cloud.Gltfast
{
    static class NativeArrayExtensions
    {
        internal static unsafe uint ReadUInt32(this NativeArray<byte>.ReadOnly data, int offset)
        {
            var ptr = (uint*)((byte*)data.GetUnsafeReadOnlyPtr() + offset);
            return *ptr;
        }
    }
}
