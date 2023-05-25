// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using UnityEngine;

#if NET_STANDARD
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
#else
using System.Collections.Generic;
#endif

namespace GLTFast.Export
{
    static class StreamExtension
    {

#if NET_STANDARD
        public static unsafe void Write(this Stream stream, NativeArray<byte> array) {
            var span = new ReadOnlySpan<byte>(array.GetUnsafeReadOnlyPtr(), array.Length);
            stream.Write(span);
        }
#else
        public static void Write(this Stream stream, IEnumerable<byte> array)
        {
            // TODO: Is there a faster way?
            foreach (var b in array)
            {
                stream.WriteByte(b);
            }
        }
#endif
    }
}
