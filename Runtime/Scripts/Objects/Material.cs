// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// The material appearance of a primitive.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class Material : NamedObject, IAdditionalPropertyContainer
    {
        /// <summary>
        /// Material extensions.
        /// </summary>
        [JsonPropertyName("extensions")]
        public MaterialExtensions Extensions { get; set; }

        /// <summary>
        /// A set of parameter values that are used to define the metallic-roughness
        /// material model from Physically-Based Rendering (PBR) methodology.
        /// </summary>
        [JsonPropertyName("pbrMetallicRoughness")]
        public PbrMetallicRoughness PbrMetallicRoughness { get; set; }

        /// <summary>
        /// A tangent space normal map. Each texel represents the XYZ components of a
        /// normal vector in tangent space.
        /// </summary>
        [JsonPropertyName("normalTexture")]
        public NormalTextureInfo NormalTexture { get; set; }

        /// <summary>
        /// The occlusion map is a greyscale texture, with white indicating areas that
        /// should receive full indirect lighting and black indicating no indirect
        /// lighting.
        /// </summary>
        [JsonPropertyName("occlusionTexture")]
        public OcclusionTextureInfo OcclusionTexture { get; set; }

        /// <summary>
        /// The emissive map controls the color and intensity of the light being emitted
        /// by the material. This texture contains RGB components in sRGB color space.
        /// If a fourth component (A) is present, it is ignored.
        /// </summary>
        [JsonPropertyName("emissiveTexture")]
        public TextureInfo EmissiveTexture { get; set; }

        /// <summary>
        /// The RGB components of the emissive color of the material.
        /// If an emissiveTexture is specified, this value is multiplied with the texel
        /// values.
        /// </summary>
        [JsonPropertyName("emissiveFactor")]
        [JsonConverter(typeof(ColorConverter))]
        public Color EmissiveFactor { get; set; } = Color.Black;

        /// <summary>
        /// The material's alpha rendering mode enumeration specifying the interpretation of the
        /// alpha value of the main factor and texture. In `OPAQUE` mode, the alpha value is
        /// ignored and the rendered output is fully opaque. In `MASK` mode, the rendered output
        /// is either fully opaque or fully transparent depending on the alpha value and the
        /// specified alpha cutoff value. In `BLEND` mode, the alpha value is used to composite
        /// the source and destination areas. The rendered output is combined with the background
        /// using the normal painting operation (i.e. the Porter and Duff over operator).
        /// </summary>
        [JsonPropertyName("alphaMode")]
        [JsonConverter(typeof(AlphaModeValueConverter))]
        public EnumOrRawValue<AlphaMode> AlphaMode { get; set; }

        /// <summary>
        /// Specifies the cutoff threshold when in `MASK` mode. If the alpha value is greater than
        /// or equal to this value then it is rendered as fully opaque, otherwise, it is rendered
        /// as fully transparent. This value is ignored for other modes.
        /// </summary>
        [JsonIgnore]
        public float AlphaCutoff { get; set; } = .5f;

        [JsonPropertyName("alphaCutoff"), JsonInclude]
        internal float? AlphaCutoffSerialized
        {
            get => Mathematics.Approximately(AlphaCutoff, .5f) ? null : AlphaCutoff;
            set => AlphaCutoff = value ?? .5f;
        }

        /// <summary>
        /// Specifies whether the material is double sided. When this value is false, back-face
        /// culling is enabled. When this value is true, back-face culling is disabled and double
        /// sided lighting is enabled. The back-face must have its normals reversed before the
        /// lighting equation is evaluated.
        /// </summary>
        [JsonPropertyName("doubleSided")]
        public bool DoubleSided { get; set; }

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


        /// <summary>
        /// True if the material requires the mesh to have normals.
        /// </summary>
        [JsonIgnore]
        public bool RequiresNormals => Extensions?.Unlit == null;

        /// <summary>
        /// True if the material requires the mesh to have tangents.
        /// </summary>
        [JsonIgnore]
        public bool RequiresTangents => NormalTexture is { Index: >= 0 };
    }
}
