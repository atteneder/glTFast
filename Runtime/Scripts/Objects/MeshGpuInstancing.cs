// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{

    /// <summary>
    /// Extension for enabling GPU instancing, rendering many copies of a
    /// single mesh at once using a small number of draw calls.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Vendor/EXT_mesh_gpu_instancing"/>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class MeshGpuInstancing
    {
        /// <inheritdoc cref="InstancesAttributes"/>
        [JsonPropertyName("attributes")]
        public InstancesAttributes Attributes { get; set; }
    }
}
