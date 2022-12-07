// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

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
