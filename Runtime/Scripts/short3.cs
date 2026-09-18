// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Unity.Cloud.Gltfast
{
    /// <summary>A 3 component vector of signed 16-bit integers.</summary>
    struct short3
    {
        /// <summary>x component of the vector.</summary>
        public short x;
        /// <summary>y component of the vector.</summary>
        public short y;
        /// <summary>z component of the vector.</summary>
        public short z;

        /// <summary>Constructs a short3 vector from three short values.</summary>
        /// <param name="x">The constructed vector's x component will be set to this value.</param>
        /// <param name="y">The constructed vector's y component will be set to this value.</param>
        /// <param name="z">The constructed vector's z component will be set to this value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short3(short x, short y, short z)
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
        /// Converts 3 component vector from signed short in glTF space to
        /// normalized float vector in Unity space. Applies normalization as last step to ensure a magnitude of 1.0.
        /// </summary>
        /// <returns>Normalized 3 component vector in Unity space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 GltfNormalToUnityFloat3()
        {
            var tmp = new float3(x, y, z) / short.MaxValue;
            tmp = math.max(tmp, -1f);
            tmp.x *= -1;
            return math.normalize(tmp);
        }

        /// <summary>
        /// Converts 3 component vector from signed short in glTF space to
        /// normalized float vector in Unity space.
        /// </summary>
        /// <returns>Normalized 3 component vector in Unity space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 GltfToUnityNormalizedFloat3()
        {
            var tmp = new float3(x, y, z) / short.MaxValue;
            tmp = math.max(tmp, -1f);
            tmp.x *= -1;
            return tmp;
        }
    }
}
