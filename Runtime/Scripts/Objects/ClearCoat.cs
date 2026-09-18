// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{

    /// <summary>
    /// This extension defines a clear coating that can be layered on top of an
    /// existing glTF material definition.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md"/>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class ClearCoat
    {

        /// <summary>
        /// The clearcoat layer intensity.
        /// </summary>
        [JsonPropertyName("clearcoatFactor")]
        public float ClearcoatFactor { get; set; }

        /// <summary>
        /// The clearcoat layer intensity texture.
        /// </summary>
        [JsonPropertyName("clearcoatTexture")]
        public TextureInfo ClearcoatTexture { get; set; }

        /// <summary>
        /// The clearcoat layer roughness.
        /// </summary>
        [JsonPropertyName("clearcoatRoughnessFactor")]
        public float ClearcoatRoughnessFactor { get; set; }

        /// <summary>
        /// The clearcoat layer roughness texture.
        /// </summary>
        [JsonPropertyName("clearcoatRoughnessTexture")]
        public TextureInfo ClearcoatRoughnessTexture { get; set; }

        /// <summary>
        /// The clearcoat normal map texture.
        /// </summary>
        [JsonPropertyName("clearcoatNormalTexture")]
        public NormalTextureInfo ClearcoatNormalTexture { get; set; }
    }
}
