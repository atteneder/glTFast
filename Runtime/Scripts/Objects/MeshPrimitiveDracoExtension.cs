// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if DRACO_IS_INSTALLED

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class MeshPrimitiveDracoExtension
    {
        [JsonPropertyName("bufferView")]
        public int? BufferView { get; set; }

        [JsonPropertyName("attributes")]
        public Attributes Attributes { get; set; }
    }
}
#endif // DRACO_IS_INSTALLED
