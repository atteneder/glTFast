// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Text.Json.Serialization;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Specifies if the accessor’s elements are scalars, vectors, or matrices.
    /// </summary>
    /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#_accessor_type"/>
    [JsonConverter(typeof(JsonStringEnumConverter<AccessorType>))]
    public enum AccessorType : byte
    {
        /// <summary>Undefined</summary>
        Undefined,

        /// <summary>Scalar. single value.</summary>
        [JsonStringEnumMemberName("SCALAR")]
        Scalar,
        /// <summary>Two component vector</summary>
        [JsonStringEnumMemberName("VEC2")]
        Vector2,
        /// <summary>Three component vector</summary>
        [JsonStringEnumMemberName("VEC3")]
        Vector3,
        /// <summary>Four component vector</summary>
        [JsonStringEnumMemberName("VEC4")]
        Vector4,
        /// <summary>2x2 matrix (4 values)</summary>
        [JsonStringEnumMemberName("MAT2")]
        Matrix2x2,
        /// <summary>3x3 matrix (9 values)</summary>
        [JsonStringEnumMemberName("MAT3")]
        Matrix3x3,
        /// <summary>4x4 matrix (16 values)</summary>
        [JsonStringEnumMemberName("MAT4")]
        Matrix4x4
    }
}
