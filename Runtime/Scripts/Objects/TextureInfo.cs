// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Reference to a texture.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class TextureInfo : IAdditionalPropertyContainer
    {
        /// <summary>
        /// The index of the texture.
        /// </summary>
        [JsonPropertyName("index")]
        public int? Index { get; set; }

        /// <summary>
        /// This integer value is used to construct a string in the format
        /// TEXCOORD_&lt;set index&gt; which is a reference to a key in
        /// mesh.primitives.attributes (e.g. A value of 0 corresponds to TEXCOORD_0).
        /// </summary>
        [JsonPropertyName("texCoord")]
        public int TexCoord { get; set; }

        /// <inheritdoc cref="TextureInfoExtensions"/>
        [JsonPropertyName("extensions")]
        public TextureInfoExtensions Extensions { get; set; }

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

        /// <summary>
        /// Applies a texture transform by initializing <see cref="Extensions" /> (if required) and setting its
        /// <see cref="TextureInfoExtensions.TextureTransform" /> field.
        /// </summary>
        /// <param name="textureTransform">Texture transform to apply.</param>
        internal void SetTextureTransform(TextureTransform textureTransform)
        {
            Extensions ??= new TextureInfoExtensions();
            Extensions.TextureTransform = textureTransform;
        }
    }
}
