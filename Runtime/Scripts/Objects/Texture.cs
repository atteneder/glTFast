// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// A texture is defined by an image and a sampler.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class Texture : NamedObject, IAdditionalPropertyContainer
    {
        /// <inheritdoc cref="TextureExtensions"/>
        [JsonPropertyName("extensions")]
        public TextureExtensions Extensions { get; set; }

        /// <summary>
        /// The index of the sampler used by this texture.
        /// </summary>
        [JsonPropertyName("sampler")]
        public int? Sampler { get; set; }

        /// <summary>
        /// The index of the image used by this texture.
        /// </summary>
        [JsonPropertyName("source")]
        public int? Source { get; set; }

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
        /// Retrieves the final image index.
        /// </summary>
        /// <returns>Final image index</returns>
        public int? GetImageIndex()
        {
            if (Extensions != null)
            {
                if (Extensions.BasisU != null && Extensions.BasisU.Source.HasValue)
                {
                    return Extensions.BasisU.Source;
                }
            }
            return Source;
        }

        /// <summary>
        /// True, if the texture is of the KTX format.
        /// </summary>
        [JsonIgnore]
        public bool IsKtx => Extensions?.BasisU != null;
    }
}
