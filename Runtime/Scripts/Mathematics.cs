// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using static Unity.Mathematics.math;

namespace Unity.Cloud.Gltfast
{

    using Unity.Mathematics;

    /// <summary>
    /// Mathematics helper methods
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public static class Mathematics
    {
        internal static readonly double3 k_Double3One = new double3(1, 1, 1);
        internal static readonly double4 k_QuaternionIdentity = new double4(0, 0, 0, 1);

        /// <summary>
        /// Decomposes a 4x4 TRS matrix into separate transforms (translation * rotation * scale)
        /// Matrix may not contain skew
        /// </summary>
        /// <param name="m">Input matrix</param>
        /// <param name="translation">Translation</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        public static void Decompose(
            this double4x4 m,
            out double3 translation,
            out double4 rotation,
            out double3 scale
            )
        {
            var mRotScale = new double3x3(
                m.c0.xyz,
                m.c1.xyz,
                m.c2.xyz
                );
            mRotScale.Decompose(out rotation, out scale);
            translation = m.c3.xyz;
        }

        /// <summary>
        /// Decomposes a 3x3 matrix into rotation and scale
        /// </summary>
        /// <param name="m">Input matrix</param>
        /// <param name="rotation">Rotation quaternion values</param>
        /// <param name="scale">Scale</param>
        static void Decompose(this double3x3 m, out double4 rotation, out double3 scale)
        {
            var lenC0 = length(m.c0);
            var lenC1 = length(m.c1);
            var lenC2 = length(m.c2);

            double3x3 rotationMatrix;
            rotationMatrix.c0 = m.c0 / lenC0;
            rotationMatrix.c1 = m.c1 / lenC1;
            rotationMatrix.c2 = m.c2 / lenC2;

            scale.x = lenC0;
            scale.y = lenC1;
            scale.z = lenC2;

            if (rotationMatrix.IsNegative())
            {
                rotationMatrix *= -1f;
                scale *= -1f;
            }

            // Inlined normalize(rotationMatrix)
            rotationMatrix.c0 = math.normalize(rotationMatrix.c0);
            rotationMatrix.c1 = math.normalize(rotationMatrix.c1);
            rotationMatrix.c2 = math.normalize(rotationMatrix.c2);

            rotation = rotationMatrix.ToQuaternion();
        }

        static bool IsNegative(this double3x3 m)
        {
            var cross = math.cross(m.c0, m.c1);
            return dot(cross, m.c2) < 0f;
        }

        /// <summary>
        /// Normalizes a vector
        /// </summary>
        /// <param name="input">Input vector</param>
        /// <param name="output">Normalized output vector</param>
        /// <returns>Length/magnitude of input vector</returns>
        public static float Normalize(float2 input, out float2 output)
        {
            var len = math.length(input);
            output = input / len;
            return len;
        }

        /// <summary>
        /// Returns double4 quaternion values that rotates around the y-axis by a given number of radians.
        /// </summary>
        /// <param name="angle">
        /// The clockwise rotation angle when looking along the y-axis towards the origin in radians.
        /// </param>
        /// <returns>Quaternion values representing a rotation around the y-axis.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double4 RotateY(double angle)
        {
            sincos(0.5 * angle, out var sina, out var cosa);
            return double4(0.0f, sina, 0.0f, cosa);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool ApproximatelyOne(float value)
        {
            return abs(value - 1) <= EPSILON;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Approximately(float value1, float value2)
        {
            return abs(value1 - value2) <= EPSILON;
        }
    }

    static class Double3Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(this double3 v)
        {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }
    }

    static class Double4Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ToUnityEngineQuaternion(this double4 v)
        {
            return new Quaternion((float)v.x, (float)v.y, (float)v.z, (float)v.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion ToQuaternion(this double4 v)
        {
            return new quaternion((float)v.x, (float)v.y, (float)v.z, (float)v.w);
        }
    }

    static class Double3x3Extensions
    {
        /// <summary>Constructs a unit quaternion from a double3x3 rotation matrix. The matrix must be orthonormal.</summary>
        /// <param name="m">The float3x3 orthonormal rotation matrix.</param>
        public static double4 ToQuaternion(this double3x3 m)
        {
            var u = m.c0;
            var v = m.c1;
            var w = m.c2;

            var u_sign = (asulong(u.x) & 0x8000000000000000);
            double t = v.y + asdouble(asulong(w.z) ^ u_sign);
            var u_mask = new ulong4((long)u_sign >> 63);
            var t_mask = new ulong4((ulong)(aslong(t) >> 63));

            double tr = 1.0f + abs(u.x);

            var sign_flips = new ulong4(0x00000000, 0x8000000000000000, 0x8000000000000000, 0x8000000000000000)
                ^ (u_mask & new ulong4(0x00000000, 0x8000000000000000, 0x00000000, 0x8000000000000000))
                ^ (t_mask & new ulong4(0x8000000000000000, 0x8000000000000000, 0x8000000000000000, 0x00000000));

            double4 value = double4(tr, u.y, w.x, v.z) + AsDouble4(AsUnsignedLong4(new double4(t, v.x, u.z, w.y)) ^ sign_flips);   // +---, +++-, ++-+, +-++

            value = AsDouble4((AsUnsignedLong4(value) & ~u_mask) | (AsUnsignedLong4(value.zwxy) & u_mask));
            value = AsDouble4((AsUnsignedLong4(value.wzyx) & ~t_mask) | (AsUnsignedLong4(value) & t_mask));
            value = normalize(value);
            return value;
        }

        /// <summary>Returns the bit pattern of a float as an int.</summary>
        /// <param name="v">The float bits to copy.</param>
        /// <returns>The int with the same bit pattern as the input.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong4 AsUnsignedLong4(double4 v)
        {
            unsafe
            {
                return *(ulong4*)&v;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double4 AsDouble4(ulong4 x)
        {
            unsafe
            {
                return *(double4*)&x;
            }
        }
    }

    static class Double4x4Extensions
    {
        public static bool ValidTRS(this double4x4 m)
        {
            return abs(m.c0.w) < EPSILON_DBL
                && abs(m.c1.w) < EPSILON_DBL
                && abs(m.c2.w) < EPSILON_DBL
                && abs(abs(m.c3.w) - 1) < EPSILON_DBL;
        }
    }

    static class QuaternionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double4 ToDouble(this Quaternion q)
        {
            return new double4(q.x, q.y, q.z, q.w);
        }
    }

    static class Vector3Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double3 ToDouble(this Vector3 v)
        {
            return new double3(v.x, v.y, v.z);
        }
    }

    static class Matrix4x4Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double4x4 ToDouble(this Matrix4x4 m)
        {
            return new double4x4(
                m.m00, m.m01, m.m02, m.m03,
                m.m10, m.m11, m.m12, m.m13,
                m.m20, m.m21, m.m22, m.m23,
                m.m30, m.m31, m.m32, m.m33
            );
        }
    }

    struct ulong4
    {
        public ulong x;
        public ulong y;
        public ulong z;
        public ulong w;

        public ulong4(long v)
        {
            x = (ulong)v;
            y = (ulong)v;
            z = (ulong)v;
            w = (ulong)v;
        }

        public ulong4(ulong v)
        {
            x = v;
            y = v;
            z = v;
            w = v;
        }

        public ulong4(ulong x, ulong y, ulong z, ulong w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>Returns the result of a componentwise bitwise not operation on a ulong4 vector.</summary>
        /// <param name="val">Value to use when computing the componentwise bitwise not.</param>
        /// <returns>ulong4 result of the componentwise bitwise not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong4 operator ~(ulong4 val) { return new ulong4(~val.x, ~val.y, ~val.z, ~val.w); }

        /// <summary>Returns the result of a componentwise bitwise and operation on two ulong4 vectors.</summary>
        /// <param name="lhs">Left hand side ulong4 to use to compute componentwise bitwise and.</param>
        /// <param name="rhs">Right hand side ulong4 to use to compute componentwise bitwise and.</param>
        /// <returns>ulong4 result of the componentwise bitwise and.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong4 operator &(ulong4 lhs, ulong4 rhs) { return new ulong4(lhs.x & rhs.x, lhs.y & rhs.y, lhs.z & rhs.z, lhs.w & rhs.w); }

        /// <summary>Returns the result of a componentwise bitwise or operation on two ulong4 vectors.</summary>
        /// <param name="lhs">Left hand side ulong4 to use to compute componentwise bitwise or.</param>
        /// <param name="rhs">Right hand side ulong4 to use to compute componentwise bitwise or.</param>
        /// <returns>ulong4 result of the componentwise bitwise or.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong4 operator |(ulong4 lhs, ulong4 rhs) { return new ulong4(lhs.x | rhs.x, lhs.y | rhs.y, lhs.z | rhs.z, lhs.w | rhs.w); }

        /// <summary>Returns the result of a componentwise bitwise exclusive or operation on two ulong4 vectors.</summary>
        /// <param name="lhs">Left hand side ulong4 to use to compute componentwise bitwise exclusive or.</param>
        /// <param name="rhs">Right hand side ulong4 to use to compute componentwise bitwise exclusive or.</param>
        /// <returns>ulong4 result of the componentwise bitwise exclusive or.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong4 operator ^(ulong4 lhs, ulong4 rhs) { return new ulong4(lhs.x ^ rhs.x, lhs.y ^ rhs.y, lhs.z ^ rhs.z, lhs.w ^ rhs.w); }
    }
}
