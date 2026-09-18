// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// BufferView extensions
    /// </summary>
    /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#reference-bufferview"/>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class BufferViewExtensions : AdditionalPropertyContainer
    {
#if MESHOPT_IS_RECENT
        [JsonPropertyName("EXT_meshopt_compression")]
        public BufferViewMeshoptExtension ExtMeshoptCompression { get; set; }
#endif
    }
}
