// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// A set of parameter values that are used to define the metallic-roughness
    /// material model from Physically-Based Rendering (PBR) methodology.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class PbrMetallicRoughness : IAdditionalPropertyContainer
    {
        /// <summary>
        /// The base color texture.
        /// This texture contains RGB(A) components in sRGB color space.
        /// The first three components (RGB) specify the base color of the material.
        /// If the fourth component (A) is present, it represents the opacity of the
        /// material. Otherwise, an opacity of 1.0 is assumed.
        /// </summary>
        [JsonPropertyName("baseColorTexture")]
        public TextureInfo BaseColorTexture { get; set; }

        /// <summary>
        /// The metallic-roughness texture has two components.
        /// The first component (R) contains the metallic-ness of the material.
        /// The second component (G) contains the roughness of the material.
        /// These values are linear.
        /// If the third component (B) and/or the fourth component (A) are present,
        /// they are ignored.
        /// </summary>
        [JsonPropertyName("metallicRoughnessTexture")]
        public TextureInfo MetallicRoughnessTexture { get; set; }

        /// <summary>
        /// The RGBA components of the base color of the material.
        /// The fourth component (A) is the opacity of the material.
        /// These values are linear.
        /// </summary>
        [JsonIgnore]
        public ColorAlpha BaseColorFactor { get; set; } = ColorAlpha.White;

        [JsonPropertyName("baseColorFactor"), JsonInclude]
        [JsonConverter(typeof(ColorAlphaConverter))]
        internal ColorAlpha? BaseColorFactorSerialized
        {
            get => BaseColorFactor == ColorAlpha.White ? null : BaseColorFactor;
            set => BaseColorFactor = value ?? ColorAlpha.White;
        }

        /// <summary>
        /// The metalness of the material.
        /// A value of 1.0 means the material is a metal.
        /// A value of 0.0 means the material is a dielectric.
        /// Values in between are for blending between metals and dielectrics such as
        /// dirty metallic surfaces.
        /// This value is linear.
        /// </summary>
        [JsonIgnore]
        public float MetallicFactor { get; set; } = 1f;

        [JsonPropertyName("metallicFactor"), JsonInclude]
        internal float? MetallicFactorSerialized
        {
            get => Mathematics.ApproximatelyOne(MetallicFactor) ? null : MetallicFactor;
            set => MetallicFactor = value ?? 1f;
        }

        /// <summary>
        /// The roughness of the material.
        /// A value of 1.0 means the material is completely rough.
        /// A value of 0.0 means the material is completely smooth.
        /// This value is linear.
        /// </summary>
        [JsonIgnore]
        public float RoughnessFactor { get; set; } = 1f;

        [JsonPropertyName("roughnessFactor"), JsonInclude]
        internal float? RoughnessFactorSerialized
        {
            get => Mathematics.ApproximatelyOne(RoughnessFactor) ? null : RoughnessFactor;
            set => RoughnessFactor = value ?? 1f;
        }

        /// <inheritdoc cref="Asset.Extensions"/>
        [JsonPropertyName("extensions")]
        public PbrMetallicRoughnessExtensions Extensions { get; set; }

        /// <inheritdoc cref="Root.Extras"/>
        [JsonPropertyName("extras")]
        [JsonConverter(typeof(ExtrasConverter))]
        public ExtrasContainer Extras { get; set; }

        /// <summary>JSON properties without a matching member.</summary>
        [JsonExtensionData, JsonInclude]
        internal Dictionary<string, JsonElement> ExtensionData { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public ReadOnlyProperties AdditionalProperties => new(ExtensionData ?? ReadOnlyProperties.Empty);

    }
}
