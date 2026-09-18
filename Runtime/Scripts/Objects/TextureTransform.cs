// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;
using Unity.Mathematics;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{

    /// <inheritdoc cref="Extension.TextureTransform"/>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class TextureTransform
    {

        /// <summary>
        /// The offset of the UV coordinate origin as a factor of the texture dimensions.
        /// </summary>
        [JsonPropertyName("offset")]
        [JsonConverter(typeof(Float2Converter))]
        public float2? Offset { get; set; }

        /// <summary>
        /// Rotate the UVs by this many radians counter-clockwise around the origin. This is equivalent to a similar rotation of the image clockwise.
        /// </summary>
        [JsonPropertyName("rotation")]
        public float Rotation { get; set; }

        /// <summary>
        /// The scale factor applied to the components of the UV coordinates.
        /// </summary>
        [JsonPropertyName("scale")]
        [JsonConverter(typeof(Float2Converter))]
        public float2? Scale { get; set; }

        /// <summary>
        /// Overrides the textureInfo texCoord value if supplied, and if this extension is supported.
        /// </summary>
        [JsonPropertyName("texCoord")]
        public int? TexCoord { get; set; }
    }
}
