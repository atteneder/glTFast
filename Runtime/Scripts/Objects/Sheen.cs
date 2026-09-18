// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{

    /// <summary>
    /// This extension defines a sheen that can be layered on top of an
    /// existing glTF material definition. A sheen layer is a common technique
    /// used in Physically-Based Rendering to represent cloth and fabric
    /// materials, for example.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_sheen"/>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class Sheen
    {

        /// <summary>
        /// The sheen color red, green and blue components in linear space.
        /// </summary>
        [JsonPropertyName("sheenColorFactor")]
        [JsonConverter(typeof(ColorConverter))]
        public Color SheenColorFactor { get; set; }

        /// <summary>
        /// The sheen color texture.
        /// </summary>
        [JsonPropertyName("sheenColorTexture")]
        public TextureInfo SheenColorTexture { get; set; }

        /// <summary>
        /// The sheen roughness.
        /// </summary>
        [JsonPropertyName("sheenRoughnessFactor")]
        public float SheenRoughnessFactor { get; set; }

        /// <summary>
        /// The sheen roughness (Alpha) texture.
        /// </summary>
        [JsonPropertyName("sheenRoughnessTexture")]
        public TextureInfo SheenRoughnessTexture { get; set; }
    }
}
