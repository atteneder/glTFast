// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Unity.Cloud.Gltfast
{
    /// <summary>A 3 component vector of unsigned bytes.</summary>
    struct byte3
    {
        /// <summary>x component of the vector.</summary>
        public byte x;
        /// <summary>y component of the vector.</summary>
        public byte y;
        /// <summary>z component of the vector.</summary>
        public byte z;

        /// <summary>Constructs a byte3 vector from three byte values.</summary>
        /// <param name="x">The constructed vector's x component will be set to this value.</param>
        /// <param name="y">The constructed vector's y component will be set to this value.</param>
        /// <param name="z">The constructed vector's z component will be set to this value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte3(byte x, byte y, byte z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Converts 3 component vector from unsigned byte in glTF space to
        /// float in Unity space.
        /// </summary>
        /// <returns>3 component vector in Unity space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 GltfToUnityFloat3()
        {
            return new float3(-x, y, z);
        }

        /// <summary>
        /// Converts 3 component vector from unsigned byte in glTF space to
        /// normalized float vector in Unity space.
        /// </summary>
        /// <returns>Normalized 3 component vector in Unity space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float3 GltfToUnityNormalizedFloat3()
        {
            return new float3(
                -(x / 255f),
                y / 255f,
                z / 255f
            );
        }

        /// <summary>
        /// Converts triangle indices from unsigned byte in glTF space to
        /// unsigned short indices in Unity space.
        /// </summary>
        /// <returns>Triangle indices vector in Unity space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort3 GltfToUnityTriangleIndicesUInt16()
        {
            return new ushort3(x, z, y);
        }

        /// <summary>
        /// Converts triangle indices from unsigned byte in glTF space to
        /// unsigned int indices in Unity space.
        /// </summary>
        /// <returns>Triangle indices vector in Unity space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint3 GltfToUnityTriangleIndices()
        {
            return new uint3(x, z, y);
        }
    }
}
