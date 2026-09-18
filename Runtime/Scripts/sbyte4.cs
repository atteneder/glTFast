// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Unity.Cloud.Gltfast
{
    /// <summary>A 4 component vector of signed bytes.</summary>
    struct sbyte4
    {
        /// <summary>x component of the vector.</summary>
        public sbyte x;
        /// <summary>y component of the vector.</summary>
        public sbyte y;
        /// <summary>z component of the vector.</summary>
        public sbyte z;
        /// <summary>w component of the vector.</summary>
        public sbyte w;

        /// <summary>Constructs an sbyte4 vector from four sbyte values.</summary>
        /// <param name="x">The constructed vector's x component will be set to this value.</param>
        /// <param name="y">The constructed vector's y component will be set to this value.</param>
        /// <param name="z">The constructed vector's z component will be set to this value.</param>
        /// <param name="w">The constructed vector's z component will be set to this value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte4(sbyte x, sbyte y, sbyte z, sbyte w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Converts glTF rotation signed byte quaternion values in to a
        /// quaternion rotation in Unity space.
        /// </summary>
        /// <returns>Rotation in Unity space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public quaternion GltfToUnityRotation()
        {
            return new quaternion(
                max(x / 127f, -1f),
                -max(y / 127f, -1f),
                -max(z / 127f, -1f),
                max(w / 127f, -1f)
            );
        }
    }
}
