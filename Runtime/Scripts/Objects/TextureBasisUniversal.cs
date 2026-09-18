// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Basis Universal texture extension
    /// </summary>
    /// <seealso cref="Extension.TextureBasisUniversal"/>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class TextureBasisUniversal
    {
        /// <summary>
        /// Index of the image which defines a reference to the KTX v2 image
        /// with Basis Universal super-compression.
        /// </summary>
        [JsonPropertyName("source")]
        public int? Source { get; set; }
    }
}
