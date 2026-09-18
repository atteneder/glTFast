// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Morph target (blend shape)
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class MorphTarget
    {
        /// <summary>Vertex position deviation accessor index.</summary>
        [JsonPropertyName("POSITION")]
        public int? Position { get; set; }
        /// <summary>Vertex normal deviation accessor index.</summary>
        [JsonPropertyName("NORMAL")]
        public int? Normal { get; set; }
        /// <summary>Vertex tangent deviation accessor index.</summary>
        [JsonPropertyName("TANGENT")]
        public int? Tangent { get; set; }
    }
}
