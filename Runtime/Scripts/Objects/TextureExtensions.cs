// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Texture extensions
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class TextureExtensions : AdditionalPropertyContainer
    {
        /// <inheritdoc cref="Extension.TextureBasisUniversal"/>
        [JsonPropertyName("KHR_texture_basisu")]
        public TextureBasisUniversal BasisU { get; set; }
    }
}
