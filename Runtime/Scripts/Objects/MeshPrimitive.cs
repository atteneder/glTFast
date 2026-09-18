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
    /// Geometry to be rendered with the given material.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class MeshPrimitive : ICloneable, IMaterialsVariantsSlot, IAdditionalPropertyContainer
    {
        /// <inheritdoc cref="MeshPrimitiveExtensions"/>
        [JsonPropertyName("extensions")]
        public MeshPrimitiveExtensions Extensions { get; set; }

        /// <summary>
        /// A dictionary object, where each key corresponds to mesh attribute semantic
        /// and each value is the index of the accessor containing attribute's data.
        /// </summary>
        [JsonPropertyName("attributes")]
        public Attributes Attributes { get; set; }

        /// <summary>
        /// The index of the accessor that contains mesh indices.
        /// When this is not defined, the primitives should be rendered without indices
        /// using `drawArrays()`. When defined, the accessor must contain indices:
        /// the `bufferView` referenced by the accessor must have a `target` equal
        /// to 34963 (ELEMENT_ARRAY_BUFFER); a `byteStride` that is tightly packed,
        /// i.e., 0 or the byte size of `componentType` in bytes;
        /// `componentType` must be 5121 (UNSIGNED_BYTE), 5123 (UNSIGNED_SHORT)
        /// or 5125 (UNSIGNED_INT), the latter is only allowed
        /// when `OES_element_index_uint` extension is used; `type` must be `\"SCALAR\"`.
        /// </summary>
        [JsonPropertyName("indices")]
        public int? Indices { get; set; }

        /// <summary>
        /// The index of the material to apply to this primitive when rendering.
        /// </summary>
        [JsonPropertyName("material")]
        public int? Material { get; set; }

        /// <summary>
        /// The type of primitives to render. All valid values correspond to WebGL enums.
        /// </summary>
        [JsonIgnore]
        public PrimitiveMode Mode { get; set; } = PrimitiveMode.Triangles;

        [JsonPropertyName("mode"), JsonInclude]
        internal PrimitiveMode? ModeSerialized
        {
            get => Mode == PrimitiveMode.Triangles ? null : Mode;
            set => Mode = value ?? PrimitiveMode.Triangles;
        }

        /// <summary>
        /// An array of Morph Targets, each  Morph Target is a dictionary mapping
        /// attributes to their deviations
        /// in the Morph Target (index of the accessor containing the attribute
        /// displacements' data).
        /// </summary>
        [JsonPropertyName("targets")]
        public List<MorphTarget> Targets { get; set; }

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


        /// <inheritdoc />
        public int? GetMaterialIndex(int variantIndex)
        {
            var mapping = Extensions?.MaterialsVariants;
            if (mapping != null && mapping.TryGetMaterialIndex(variantIndex, out var materialIndex))
            {
                return materialIndex;
            }
            return Material;
        }

#if DRACO_IS_INSTALLED
        [JsonIgnore]
        public bool IsDracoCompressed => Extensions != null && Extensions.DracoMeshCompression != null;
#endif

        /// <summary>
        /// Clones the object
        /// </summary>
        /// <returns>Member-wise clone</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
