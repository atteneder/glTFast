// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Unity.Cloud.Gltfast
{
    /// <summary>A 4 component vector of signed 16-bit integers.</summary>
    struct short4
    {
        /// <summary>x component of the vector.</summary>
        public short x;
        /// <summary>y component of the vector.</summary>
        public short y;
        /// <summary>z component of the vector.</summary>
        public short z;
        /// <summary>w component of the vector.</summary>
        public short w;

        /// <summary>
        /// Converts glTF rotation signed short quaternion values in to a
        /// quaternion rotation in Unity space.
        /// </summary>
        /// <returns>Rotation in Unity space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public quaternion GltfToUnityRotation()
        {
            return new quaternion(
                max(x / (float)short.MaxValue, -1f),
                -max(y / (float)short.MaxValue, -1f),
                -max(z / (float)short.MaxValue, -1f),
                max(w / (float)short.MaxValue, -1f)
            );
        }
    }
}
