// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// glTF animation interpolation algorithm.
    /// </summary>
    /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#_animation_sampler_interpolation"/>
    [JsonConverter(typeof(JsonStringEnumConverter<Interpolation>))]
    public enum Interpolation
    {
        /// <summary>The animated values are linearly interpolated between keyframes.</summary>
        [JsonStringEnumMemberName("LINEAR")]
        Linear,
        /// <summary>The animated values remain constant to the output of the first keyframe, until the next keyframe.</summary>
        [JsonStringEnumMemberName("STEP")]
        Step,
        /// <summary>The animation’s interpolation is computed using a cubic spline with specified tangents.</summary>
        [JsonStringEnumMemberName("CUBICSPLINE")]
        CubicSpline
    }
}
