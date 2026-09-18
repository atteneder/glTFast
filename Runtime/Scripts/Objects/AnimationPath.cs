// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION || GLTFAST_ANIMATION

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// The name of the node’s TRS property to animate, or the "weights" of the Morph Targets it instantiates.
    /// </summary>
    /// <remarks>
    /// For the "translation" property, the values that are provided by the sampler are the translation
    /// along the X, Y, and Z axes. For the "rotation" property, the values are a quaternion in the order (x, y, z, w),
    /// where w is the scalar. For the "scale" property, the values are the scaling factors along the X, Y, and Z axes.
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter<AnimationPath>))]
    public enum AnimationPath
    {
        /// <summary>Undefined</summary>
        Undefined,
        /// <summary>Node's translation</summary>
        [JsonStringEnumMemberName("translation")]
        Translation,
        /// <summary>Node's rotation</summary>
        [JsonStringEnumMemberName("rotation")]
        Rotation,
        /// <summary>Node's scale</summary>
        [JsonStringEnumMemberName("scale")]
        Scale,
        /// <summary>Morph targets weights</summary>
        [JsonStringEnumMemberName("weights")]
        Weights,
        /// <summary>
        /// Indicates usage of extension
        /// <a href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_animation_pointer">KHR_animation_pointer</a>
        /// </summary>
        [JsonStringEnumMemberName("pointer")]
        Pointer
    }
}
#endif // UNITY_ANIMATION || GLTFAST_ANIMATION
