// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// An orthographic camera containing properties to create an orthographic projection matrix.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class CameraOrthographic : IAdditionalPropertyContainer
    {
        /// <summary>
        /// The floating-point horizontal magnification of the view. Must not be zero.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        [JsonPropertyName("xmag")]
        public float Xmag { get; set; }

        /// <summary>
        /// The floating-point vertical magnification of the view. Must not be zero.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        [JsonPropertyName("ymag")]
        public float Ymag { get; set; }

        /// <summary>
        /// The floating-point distance to the far clipping plane.
        /// <see cref="Zfar"/> must be greater than <see cref="Znear"/>.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        [JsonPropertyName("zfar")]
        public float Zfar { get; set; }

        /// <summary>
        /// The floating-point distance to the near clipping plane.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        [JsonPropertyName("znear")]
        public float Znear { get; set; }

        /// <inheritdoc cref="Asset.Extensions"/>
        [JsonPropertyName("extensions")]
        public CameraOrthographicExtensions Extensions { get; set; }

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
