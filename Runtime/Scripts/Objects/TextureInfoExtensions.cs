// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{

    /// <summary>
    /// TextureInfo extensions
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class TextureInfoExtensions : AdditionalPropertyContainer
    {
        /// <inheritdoc cref="Extension.TextureTransform"/>
        [JsonPropertyName("KHR_texture_transform")]
        public TextureTransform TextureTransform { get; set; }
    }
}
