// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{

    /// <summary>
    /// A camera’s projection
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class Camera : NamedObject, IAdditionalPropertyContainer
    {
        /// <inheritdoc cref="CameraOrthographic"/>
        [JsonPropertyName("orthographic")]
        public CameraOrthographic Orthographic { get; set; }

        /// <inheritdoc cref="CameraPerspective"/>
        [JsonPropertyName("perspective")]
        public CameraPerspective Perspective { get; set; }

        /// <inheritdoc cref="CameraType"/>
        [JsonPropertyName("type")]
        [JsonConverter(typeof(CameraTypeValueConverter))]
        public EnumOrRawValue<CameraType> Type { get; set; }

        /// <inheritdoc cref="Asset.Extensions"/>
        [JsonPropertyName("extensions")]
        public CameraExtensions Extensions { get; set; }

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

    }
}
