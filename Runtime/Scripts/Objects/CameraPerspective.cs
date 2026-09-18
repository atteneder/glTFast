// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Unity.Cloud.Gltfast.Text.Json;
using Unity.Cloud.Gltfast.Text.Json.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// A perspective camera containing properties to create a perspective projection matrix.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Schema", sourceAssembly: "glTFast")]
    public class CameraPerspective : IAdditionalPropertyContainer
    {

        /// <summary>
        /// The floating-point aspect ratio of the field of view.
        /// </summary>
        [JsonPropertyName("aspectRatio")]
        public float? AspectRatio { get; set; }

        /// <summary>
        /// The floating-point vertical field of view in radians.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        [JsonPropertyName("yfov")]
        public float Yfov { get; set; }

        /// <summary>
        /// The floating-point distance to the far clipping plane.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        [JsonPropertyName("zfar")]
        public float? Zfar { get; set; }

        /// <summary>
        /// The floating-point distance to the near clipping plane.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        [JsonPropertyName("znear")]
        public float Znear { get; set; }

        /// <inheritdoc cref="Asset.Extensions"/>
        [JsonPropertyName("extensions")]
        public CameraPerspectiveExtensions Extensions { get; set; }

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
