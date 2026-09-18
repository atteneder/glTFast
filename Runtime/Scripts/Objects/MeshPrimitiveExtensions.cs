// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Mesh primitive extensions
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class MeshPrimitiveExtensions : AdditionalPropertyContainer
    {
#if DRACO_IS_INSTALLED
        [JsonPropertyName("KHR_draco_mesh_compression")]
        public MeshPrimitiveDracoExtension DracoMeshCompression { get; set; }
#endif

        /// <inheritdoc cref="MaterialsVariantsMeshPrimitiveExtension"/>
        [JsonPropertyName("KHR_materials_variants")]
        public MaterialsVariantsMeshPrimitiveExtension MaterialsVariants { get; set; }
    }
}
