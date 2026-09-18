// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{

    /// <summary>
    /// Joints and matrices defining a skinned mesh.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class Skin : NamedObject, IAdditionalPropertyContainer
    {
        /// <summary>
        /// The index of the accessor containing the
        /// floating-point 4x4 inverse-bind matrices.
        /// </summary>
        [JsonPropertyName("inverseBindMatrices")]
        public int? InverseBindMatrices { get; set; }

        /// <summary>
        /// The index of the node used as a skeleton root.
        /// </summary>
        [JsonPropertyName("skeleton")]
        public int? Skeleton { get; set; }

        /// <summary>
        /// Indices of skeleton nodes, used as joints in this skin.
        /// </summary>
        [JsonPropertyName("joints")]
        public List<uint> Joints { get; set; }

        /// <inheritdoc cref="Asset.Extensions"/>
        [JsonPropertyName("extensions")]
        public SkinExtensions Extensions { get; set; }

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
