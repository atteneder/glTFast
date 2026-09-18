// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// A set of primitives to be rendered. Its global transform is defined by
    /// a node that references it.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class Mesh : NamedObject, IAdditionalPropertyContainer, ICloneable
    {
        /// <summary>
        /// An array of primitives, each defining geometry to be rendered with
        /// a material.
        /// </summary>
        [JsonPropertyName("primitives")]
        public List<MeshPrimitive> Primitives { get; set; }

        /// <inheritdoc cref="MeshExtras"/>
        [JsonPropertyName("extras")]
        [JsonConverter(typeof(MeshExtrasConverter))]
        public MeshExtras Extras { get; set; }

        /// <summary>
        /// Clones the Mesh object
        /// </summary>
        /// <returns>Member-wise clone</returns>
        public object Clone()
        {
            var clone = (Mesh)MemberwiseClone();
            if (Primitives != null)
            {
                clone.Primitives = new List<MeshPrimitive>(Primitives.Count);
                for (var i = 0; i < Primitives.Count; i++)
                {
                    clone.Primitives.Add((MeshPrimitive)Primitives[i].Clone());
                }
            }
            return clone;
        }

        /// <summary>
        /// Array of weights to be applied to the Morph Targets.
        /// </summary>
        [JsonPropertyName("weights")]
        [JsonConverter(typeof(FloatListConverter))]
        public List<float> Weights { get; set; }

        /// <inheritdoc cref="Asset.Extensions"/>
        [JsonPropertyName("extensions")]
        public MeshExtensions Extensions { get; set; }

        /// <summary>JSON properties without a matching member.</summary>
        [JsonExtensionData, JsonInclude]
        internal Dictionary<string, JsonElement> ExtensionData { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public ReadOnlyProperties AdditionalProperties => new(ExtensionData ?? ReadOnlyProperties.Empty);

    }
}
