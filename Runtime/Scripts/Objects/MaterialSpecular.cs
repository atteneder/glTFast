// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// This extension allows configuring the specular reflection.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_specular"/>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class MaterialSpecular
    {
        /// <summary>
        /// The strength of the specular reflection.
        /// </summary>
        [JsonIgnore]
        public float SpecularFactor { get; set; } = 1f;

        [JsonPropertyName("specularFactor"), JsonInclude]
        internal float? SpecularFactorSerialized
        {
            get => Mathematics.ApproximatelyOne(SpecularFactor) ? null : SpecularFactor;
            set => SpecularFactor = value ?? 1f;
        }

        /// <summary>
        /// A texture that defines the strength of the specular reflection, stored in the alpha (A) channel.
        /// This will be multiplied by specularFactor.
        /// </summary>
        [JsonPropertyName("specularTexture")]
        public TextureInfo SpecularTexture { get; set; }

        /// <summary>
        /// The F0 color of the specular reflection (linear RGB).
        /// </summary>
        [JsonIgnore]
        public Color SpecularColorFactor { get; set; } = Color.White;

        [JsonPropertyName("specularColorFactor"), JsonInclude]
        [JsonConverter(typeof(ColorConverter))]
        internal Color? SpecularColorFactorSerialized
        {
            get => SpecularColorFactor == Color.White ? null : SpecularColorFactor;
            set => SpecularColorFactor = value ?? Color.White;
        }

        /// <summary>
        /// A texture that defines the F0 color of the specular reflection, stored in the RGB channels and encoded in
        /// sRGB. This texture will be multiplied by specularColorFactor.
        /// </summary>
        [JsonPropertyName("specularColorTexture")]
        public TextureInfo SpecularColorTexture { get; set; }
    }
}
