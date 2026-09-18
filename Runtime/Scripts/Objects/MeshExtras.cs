// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Application-specific data for meshes
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class MeshExtras : ExtrasContainer
    {
        List<string> m_TargetNames;

        /// <summary>
        /// Morph targets' names
        /// </summary>
        /// <remarks>Setting this turns the <c>extras</c> into a JSON object, discarding a non-object
        /// <see cref="ExtrasContainer.RawValue"/>.</remarks>
        [JsonPropertyName("targetNames")]
        public List<string> TargetNames
        {
            get => m_TargetNames;
            set
            {
                RawValueElement = default;
                m_TargetNames = value;
            }
        }
    }
}
