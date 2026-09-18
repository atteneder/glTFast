// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Unity.Cloud.Gltfast
{
    /// <summary>A 3 component vector of signed bytes.</summary>
    struct sbyte3
    {
        /// <summary>x component of the vector.</summary>
        public sbyte x;
        /// <summary>y component of the vector.</summary>
        public sbyte y;
        /// <summary>z component of the vector.</summary>
        public sbyte z;

        /// <summary>Constructs an sbyte3 vector from three sbyte values.</summary>
        /// <param name="x">The constructed vector's x component will be set to this value.</param>
        /// <param name="y">The constructed vector's y component will be set to this value.</param>
        /// <param name="z">The constructed vector's z component will be set to this value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte3(sbyte x, sbyte y, sbyte z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Converts 3 component vector from signed byte in glTF space to
        /// float in Unity space.
        /// </summary>
        /// <returns>3 component vector in Unity space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 GltfToUnityFloat3()
        {
            return new float3(-x, y, z);
        }

        /// <summary>
        /// Converts 3 component vector from signed byte in glTF space to
        /// normalized float vector in Unity space. Applies normalization as last step to ensure a magnitude of 1.0.
        /// </summary>
        /// <returns>Normalized 3 component vector in Unity space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 GltfNormalToUnityFloat3()
        {
            var tmp = new float3(x, y, z) / 127f;
            tmp = math.max(tmp, -1f);
            tmp.x *= -1;
            return math.normalize(tmp);
        }

        /// <summary>
        /// Converts 3 component vector from unsigned byte in glTF space to
        /// normalized float vector in Unity space.
        /// </summary>
        /// <returns>Normalized 3 component vector in Unity space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 GltfToUnityNormalizedFloat3()
        {
            var tmp = new float3(x, y, z) / 127f;
            tmp = math.max(tmp, -1f);
            tmp.x *= -1;
            return tmp;
        }
    }
}
