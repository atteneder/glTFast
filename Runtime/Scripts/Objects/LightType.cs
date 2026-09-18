// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Light type
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<LightType>))]
    public enum LightType
    {
        /// <summary>Undefined</summary>
        Undefined,
        /// <summary>Spotlight</summary>
        [JsonStringEnumMemberName("spot")]
        Spot,
        /// <summary>Directional light</summary>
        [JsonStringEnumMemberName("directional")]
        Directional,
        /// <summary>Point light</summary>
        [JsonStringEnumMemberName("point")]
        Point,
    }
}
