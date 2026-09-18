// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if MESHOPT_IS_RECENT
using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class BufferViewMeshoptExtension : IBufferView
    {
        /// <summary>
        /// The index of the buffer.
        /// </summary>
        [JsonPropertyName("buffer")]
        public int? Buffer { get; set; }

        /// <summary>
        /// The offset into the buffer in bytes.
        /// </summary>
        [JsonPropertyName("byteOffset")]
        public int ByteOffset { get; set; }

        /// <summary>
        /// The length of the bufferView in bytes.
        /// </summary>
        [JsonPropertyName("byteLength")]
        public int ByteLength { get; set; }

        /// <summary>
        /// The stride, in bytes, between vertex attributes or other interleaved data.
        /// </summary>
        [JsonPropertyName("byteStride")]
        public int? ByteStride { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("mode")]
        [JsonConverter(typeof(MeshoptModeConverter))]
        public MeshoptMode Mode { get; set; } = MeshoptMode.Undefined;

        [JsonPropertyName("filter")]
        [JsonConverter(typeof(MeshoptFilterConverter))]
        public MeshoptFilter Filter { get; set; } = MeshoptFilter.None;
    }
}

#endif
