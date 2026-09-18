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
    /// Mesh vertex attribute collection. Each property value is the index of
    /// the accessor containing attribute’s data.
    /// </summary>
    [JsonConverter(typeof(AttributesConverter))]
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class Attributes : IAdditionalPropertyContainer
    {
        /// <summary>Vertex position accessor index.</summary>
        public int? Position { get; set; }

        /// <summary>Vertex normals accessor index.</summary>
        public int? Normal { get; set; }

        /// <summary>Vertex tangents accessor index.</summary>
        public int? Tangent { get; set; }

        /// <summary>
        /// Texture coordinate accessor indices. List index <c>n</c> corresponds
        /// to the glTF semantic <c>TEXCOORD_n</c>. Sparse holes are
        /// <see langword="null"/>. Use the
        /// <see cref="AttributesExtensions.GetTexCoord"/>/<see cref="AttributesExtensions.SetTexCoord"/>
        /// extension methods for bounds-checked index access.
        /// </summary>
        public List<int?> TexCoords { get; set; }

        /// <summary>
        /// Vertex color accessor indices (<c>COLOR_n</c>). Same semantics as
        /// <see cref="TexCoords"/>; see
        /// <see cref="AttributesExtensions.GetColor"/>/<see cref="AttributesExtensions.SetColor"/>.
        /// </summary>
        public List<int?> Colors { get; set; }

        /// <summary>
        /// Bone joint accessor indices (<c>JOINTS_n</c>). Same semantics as
        /// <see cref="TexCoords"/>; see
        /// <see cref="AttributesExtensions.GetJoint"/>/<see cref="AttributesExtensions.SetJoint"/>.
        /// </summary>
        public List<int?> Joints { get; set; }

        /// <summary>
        /// Bone weight accessor indices (<c>WEIGHTS_n</c>). Same semantics as
        /// <see cref="TexCoords"/>; see
        /// <see cref="AttributesExtensions.GetWeight"/>/<see cref="AttributesExtensions.SetWeight"/>.
        /// </summary>
        public List<int?> Weights { get; set; }

        /// <summary>JSON properties without a matching member (e.g. application-defined attribute semantics such as <c>_TEMPERATURE</c>).</summary>
        [JsonExtensionData, JsonInclude]
        internal Dictionary<string, JsonElement> ExtensionData { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public ReadOnlyProperties AdditionalProperties => new(ExtensionData ?? ReadOnlyProperties.Empty);


        /// <summary>
        /// Consolidates all `TEXCOORD_*` accessor fields into a single array.
        /// The result is delimited to the maximum count of texture coordinate sets Unity supports.
        /// </summary>
        /// <param name="uvAccessors">Resulting array of accessor indices.</param>
        /// <param name="limitExceeded">If true, the attributes has more UV sets than Unity supports
        /// and uvAccessors is delimited.</param>
        /// <returns>True if there's one or more UV sets and the result is valid. False otherwise.</returns>
        [Obsolete("Access TexCoords directly instead")]
        public bool TryGetAllUVAccessors(out int[] uvAccessors, out bool limitExceeded)
        {
            var uvCount = TexCoords?.Count ?? 0;
            if (uvCount < 1)
            {
                uvAccessors = null;
                limitExceeded = false;
                return false;
            }

            limitExceeded = uvCount > VertexBufferGeneratorBase.maxUvSetCount;
            if (limitExceeded)
            {
                uvCount = VertexBufferGeneratorBase.maxUvSetCount;
            }

            uvAccessors = new int[uvCount];
            for (var i = 0; i < uvCount; i++)
            {
                uvAccessors[i] = TexCoords[i] ?? -1;
            }

            return true;
        }
    }
}
