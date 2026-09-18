// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Camera projection type
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<CameraType>))]
    public enum CameraType
    {
        /// <summary>Undefined</summary>
        Undefined,

        /// <summary>Orthogonal projection</summary>
        [JsonStringEnumMemberName("orthographic")]
        Orthographic,

        /// <summary>Perspective projection</summary>
        [JsonStringEnumMemberName("perspective")]
        Perspective
    }
}
