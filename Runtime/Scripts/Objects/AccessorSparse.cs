// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Sparse property of a glTF
    /// </summary>
    /// <seealso cref="Accessor"/>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class AccessorSparse : IAdditionalPropertyContainer
    {
        /// <summary>
        /// Number of entries stored in the sparse array.
        /// </summary>
        [JsonPropertyName("count")]
        public int Count { get; set; }

        /// <summary>
        /// Index array of size `count` that points to those accessor attributes that
        /// deviate from their initialization value. Indices must strictly increase.
        /// </summary>
        [JsonPropertyName("indices")]
        public AccessorSparseIndices Indices { get; set; }

        /// <summary>
        /// "Array of size `count` times number of components, storing the displaced
        /// accessor attributes pointed by `indices`. Substituted values must have
        /// the same `componentType` and number of components as the base accessor.
        /// </summary>
        [JsonPropertyName("values")]
        public AccessorSparseValues Values { get; set; }

        /// <inheritdoc cref="Asset.Extensions"/>
        [JsonPropertyName("extensions")]
        public AccessorSparseExtensions Extensions { get; set; }

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
