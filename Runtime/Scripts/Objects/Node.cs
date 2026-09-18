// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using Unity.Mathematics;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// An object defining the hierarchy relations and the local transform of
    /// its content.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class Node : NamedObject, IAdditionalPropertyContainer
    {
        /// <inheritdoc cref="NodeExtensions"/>
        [JsonPropertyName("extensions")]
        public NodeExtensions Extensions { get; set; }

        /// <summary>
        /// The indices of this node's children.
        /// </summary>
        [JsonPropertyName("children")]
        public List<uint> Children { get; set; }

        /// <summary>
        /// The index of the mesh in this node.
        /// </summary>
        [JsonPropertyName("mesh")]
        public int? Mesh { get; set; }

        /// <summary>
        /// A floating-point 4x4 transformation matrix stored in column-major order.
        /// </summary>
        [JsonPropertyName("matrix")]
        [JsonConverter(typeof(Double4x4Converter))]
        public double4x4? Matrix { get; set; }

        /// <summary>
        /// The node's unit quaternion rotation in the order (x, y, z, w),
        /// where w is the scalar.
        /// </summary>
        [JsonPropertyName("rotation")]
        [JsonConverter(typeof(Double4Converter))]
        public double4? Rotation { get; set; }

        /// <summary>
        /// The node's non-uniform scale.
        /// </summary>
        [JsonPropertyName("scale")]
        [JsonConverter(typeof(Double3Converter))]
        public double3? Scale { get; set; }

        /// <summary>
        /// The node's translation.
        /// </summary>
        [JsonPropertyName("translation")]
        [JsonConverter(typeof(Double3Converter))]
        public double3? Translation { get; set; }

        /// <summary>
        /// The weights of the instantiated Morph Target.
        /// Number of elements must match number of Morph Targets of used mesh.
        /// </summary>
        [JsonPropertyName("weights")]
        [JsonConverter(typeof(FloatListConverter))]
        public List<float> Weights { get; set; }

        /// <summary>
        /// The index of the skin (in <see cref="Root.Skins"/>) referenced by this node.
        /// </summary>
        [JsonPropertyName("skin")]
        public int? Skin { get; set; }

        /// <summary>
        /// Camera index
        /// </summary>
        [JsonPropertyName("camera")]
        public int? Camera { get; set; }

        /// <summary>
        /// Application-specific data.
        /// </summary>
        /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#reference-extras"/>
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
