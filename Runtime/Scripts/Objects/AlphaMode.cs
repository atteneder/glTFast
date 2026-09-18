// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// The material’s alpha rendering mode enumeration specifying the
    /// interpretation of the alpha value of the base color.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<AlphaMode>))]
    public enum AlphaMode
    {
        /// <summary>
        /// The alpha value is ignored, and the rendered output is fully
        /// opaque.
        /// </summary>
        [JsonStringEnumMemberName("OPAQUE")]
        Opaque,

        /// <summary>
        /// The rendered output is either fully opaque or fully transparent
        /// depending on the alpha value and the specified alphaCutoff
        /// value
        /// </summary>
        [JsonStringEnumMemberName("MASK")]
        Mask,

        /// <summary>
        /// The alpha value is used to composite the source and destination
        /// areas. The rendered output is combined with the background
        /// using the normal painting operation.
        /// </summary>
        [JsonStringEnumMemberName("BLEND")]
        Blend
    }
}
