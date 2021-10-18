﻿// Copyright 2020-2021 Andreas Atteneder
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

using System.ComponentModel;
using Newtonsoft.Json;

namespace GLTFast.Schema {

    public enum BufferViewTarget
    {
        None = 0,
        ArrayBuffer = 34962,
        ElementArrayBuffer = 34963,
    }

    [System.Serializable]
    public class BufferSlice {
        /// <summary>
        /// The offset into the buffer in bytes.
        /// <minimum>0</minimum>
        /// </summary>
        [DefaultValue(0)]
        public int byteOffset;

        /// <summary>
        /// The length of the bufferView in bytes.
        /// <minimum>0</minimum>
        /// </summary>
        public int byteLength;
    }

    [System.Serializable]
    public class BufferView : BufferSlice {
        /// <summary>
        /// The index of the buffer.
        /// </summary>
        [DefaultValue(-1)]
        public int buffer;

        /// <summary>
        /// The stride, in bytes, between vertex attributes or other interleavable data.
        /// When this is zero, data is tightly packed.
        /// <minimum>0</minimum>
        /// <maximum>255</maximum>
        /// </summary>
        [DefaultValue(-1)]
        public int byteStride = -1;

        /// <summary>
        /// The target that the WebGL buffer should be bound to.
        /// All valid values correspond to WebGL enums.
        /// When this is not provided, the bufferView contains animation or skin data.
        /// </summary>
        public int target;
        
        public void GltfSerialize(JsonWriter writer) {
            writer.AddObject();
            writer.AddProperty("buffer", buffer);
            writer.AddProperty("byteLength", byteLength);
            if (target > 0) {
                writer.AddProperty("target", target);
            }
            if (byteOffset > 0) {
                writer.AddProperty("byteOffset", byteOffset);
            }
            if (byteStride > 0) {
                writer.AddProperty("byteStride", byteStride);
            }
            writer.Close();
        }
    }
}