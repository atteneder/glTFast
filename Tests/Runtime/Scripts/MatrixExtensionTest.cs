// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Profiling;
using UnityEngine.TestTools.Utils;

namespace GLTFast.Tests
{
    class MatrixExtensionTest
    {
        static Vector3EqualityComparer s_Vector3Comparer;
        static QuaternionEqualityComparer s_QuaternionComparer;

        static Matrix4x4 s_UnityMatrix;
        static float4x4 s_Matrix;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            s_Vector3Comparer = new Vector3EqualityComparer(10e-6f);
            s_QuaternionComparer = new QuaternionEqualityComparer(10e-6f);

            // Corner case matrix (90°/0°/45° rotation with -1/-1/-1 scale)
            s_UnityMatrix = new Matrix4x4(
                new Vector4(
                    -0.7071067811865474f,
                    0f,
                    -0.7071067811865477f,
                    0f
                ),
                new Vector4(
                    0.7071067811865477f,
                    0f,
                    -0.7071067811865474f,
                    0f
                ),
                new Vector4(
                    0f,
                    1f,
                    0f,
                    0f
                ),
                new Vector4(
                    0f,
                    0f,
                    0f,
                    1f
                )
            );

            s_Matrix = new float4x4(
                s_UnityMatrix.m00, s_UnityMatrix.m01, s_UnityMatrix.m02, s_UnityMatrix.m03,
                s_UnityMatrix.m10, s_UnityMatrix.m11, s_UnityMatrix.m12, s_UnityMatrix.m13,
                s_UnityMatrix.m20, s_UnityMatrix.m21, s_UnityMatrix.m22, s_UnityMatrix.m23,
                s_UnityMatrix.m30, s_UnityMatrix.m31, s_UnityMatrix.m32, s_UnityMatrix.m33
            );
        }

        [Test]
        public void MatrixDecomposeTest()
        {
            Profiler.BeginSample("Matrix4x4.DecomposeUnity");
            if (s_UnityMatrix.ValidTRS())
            {
                // ReSharper disable UnusedVariable
                var t1 = new Vector3(s_UnityMatrix.m03, s_UnityMatrix.m13, s_UnityMatrix.m23);
                var r1 = s_UnityMatrix.rotation;
                var s1 = s_UnityMatrix.lossyScale;
                // ReSharper restore UnusedVariable
            }

            Profiler.EndSample();

            Profiler.BeginSample("Matrix4x4.DecomposeCustom");
            s_UnityMatrix.Decompose(out var t, out var r, out var s);
            Profiler.EndSample();

            Assert.That(t, Is.EqualTo(new Vector3(0, 0, 0)).Using(s_Vector3Comparer));
            Assert.That(r, Is.EqualTo(
                new Quaternion(0.65328151f, -0.270598054f, 0.270598054f, 0.65328151f))
                .Using(s_QuaternionComparer)
            );
            Assert.That(s, Is.EqualTo(new Vector3(-.99999994f, -.99999994f, -1)).Using(s_Vector3Comparer));

            Profiler.BeginSample("float4x4.Decompose");
            s_Matrix.Decompose(out var t3, out quaternion r3, out var s3);
            Profiler.EndSample();

            Assert.That((Vector3)t3, Is.EqualTo(new Vector3(0, 0, 0)).Using(s_Vector3Comparer));
            Assert.That(
                (Quaternion)r3,
                Is.EqualTo(new Quaternion(0.65328151f, -0.270598054f, 0.270598054f, 0.65328151f))
                    .Using(s_QuaternionComparer)
                );
            Assert.That(
                (Vector3)s3,
                Is.EqualTo(new Vector3(-.99999994f, -.99999994f, -1))
                    .Using(s_Vector3Comparer)
                );
        }

        [Test]
        public void MatrixDecomposeObsoleteTest()
        {
            Profiler.BeginSample("float4x4.Decompose");
#pragma warning disable CS0618 // Type or member is obsolete
            s_Matrix.Decompose(out var translation, out float4 rotationValues, out var scale);
#pragma warning restore CS0618 // Type or member is obsolete
            Profiler.EndSample();

            Assert.That((Vector3)translation, Is.EqualTo(new Vector3(0, 0, 0)).Using(s_Vector3Comparer));
            Assert.That(
                (Quaternion)new quaternion(rotationValues),
                Is.EqualTo(new Quaternion(0.65328151f, -0.270598054f, 0.270598054f, 0.65328151f))
                    .Using(s_QuaternionComparer)
                );
            Assert.That(
                (Vector3)scale,
                Is.EqualTo(new Vector3(-.99999994f, -.99999994f, -1))
                    .Using(s_Vector3Comparer)
                );
        }

        // [Test]
        // public void VertexStructTest() {
        //     var v = new VPosNormTan {
        //         position = new float3(1, 2, 3),
        //         normal = new float3(1, 0, 0),
        //         tangent = new float4(0, 1, 0,1),
        //     };
        //
        //     var vPosNor = new VPosNorm {
        //         position = new float3(1, 2, 3),
        //         normal = new float3(1, 0, 0),
        //     };
        //
        //     var vPos = new VPos {
        //         position = new float3(1, 2, 3)
        //     };
        //
        //     var uv1 = new VTexCoord1 {
        //         uv0 = new float2(1, 2),
        //     };
        //
        //     var uv2 = new VTexCoord2 {
        //         uv0 = new float2(1, 2),
        //         uv1 = new float2(1, 2),
        //     };
        //
        //     var uv3 = new VTexCoord3 {
        //         uv0 = new float2(1, 2),
        //         uv1 = new float2(1, 2),
        //         uv2 = new float2(1, 2),
        //     };
        //
        //     var uv4 = new VTexCoord4 {
        //         uv0 = new float2(1, 2),
        //         uv1 = new float2(1, 2),
        //         uv2 = new float2(1, 2),
        //         uv3 = new float2(1, 2),
        //     };
        //
        //     var uv5 = new VTexCoord5 {
        //         uv0 = new float2(1, 2),
        //         uv1 = new float2(1, 2),
        //         uv2 = new float2(1, 2),
        //         uv3 = new float2(1, 2),
        //         uv4 = new float2(1, 2),
        //     };
        //
        //     var uv6 = new VTexCoord6 {
        //         uv0 = new float2(1, 2),
        //         uv1 = new float2(1, 2),
        //         uv2 = new float2(1, 2),
        //         uv3 = new float2(1, 2),
        //         uv4 = new float2(1, 2),
        //         uv5 = new float2(1, 2),
        //     };
        //
        //     var uv7 = new VTexCoord7 {
        //         uv0 = new float2(1, 2),
        //         uv1 = new float2(1, 2),
        //         uv2 = new float2(1, 2),
        //         uv3 = new float2(1, 2),
        //         uv4 = new float2(1, 2),
        //         uv5 = new float2(1, 2),
        //         uv6 = new float2(1, 2),
        //     };
        //
        //     var uv8 = new VTexCoord8 {
        //         uv0 = new float2(1, 2),
        //         uv1 = new float2(1, 2),
        //         uv2 = new float2(1, 2),
        //         uv3 = new float2(1, 2),
        //         uv4 = new float2(1, 2),
        //         uv5 = new float2(1, 2),
        //         uv6 = new float2(1, 2),
        //         uv7 = new float2(1, 2),
        //     };
        // }
    }
}
