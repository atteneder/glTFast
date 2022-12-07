// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Profiling;
using UnityEngine.TestTools.Utils;

namespace GLTFast.Tests
{
    class MatrixExtensionTest
    {

        [Test]
        public void MatrixDecomposeTest()
        {
            // Corner case matrix (90°/0°/45° rotation with -1/-1/-1 scale)
            var m = new Matrix4x4(
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

            var m2 = new float4x4(
                m.m00, m.m01, m.m02, m.m03,
                m.m10, m.m11, m.m12, m.m13,
                m.m20, m.m21, m.m22, m.m23,
                m.m30, m.m31, m.m32, m.m33
            );

            Profiler.BeginSample("Matrix4x4.DecomposeUnity");
            if (m.ValidTRS())
            {
                // ReSharper disable UnusedVariable
                var t1 = new Vector3(m.m03, m.m13, m.m23);
                var r1 = m.rotation;
                var s1 = m.lossyScale;
                // ReSharper restore UnusedVariable
            }

            Profiler.EndSample();

            Profiler.BeginSample("Matrix4x4.DecomposeCustom");
            m.Decompose(out var t, out var r, out var s);
            Profiler.EndSample();

            var comparer3 = new Vector3EqualityComparer(10e-6f);
            var comparer4 = new QuaternionEqualityComparer(10e-6f);

            Assert.That(t, Is.EqualTo(new Vector3(0, 0, 0)).Using(comparer3));
            Assert.That(r, Is.EqualTo(new Quaternion(0.65328151f, -0.270598054f, 0.270598054f, 0.65328151f)).Using(comparer4));
            Assert.That(s, Is.EqualTo(new Vector3(-.99999994f, -.99999994f, -1)).Using(comparer3));

            Profiler.BeginSample("float4x4.Decompose");
            m2.Decompose(out var t3, out var r3, out var s3);
            Profiler.EndSample();

            Assert.That((Vector3)t3, Is.EqualTo(new Vector3(0, 0, 0)).Using(comparer3));
            Assert.That((Quaternion)new quaternion(r3), Is.EqualTo(new Quaternion(0.65328151f, -0.270598054f, 0.270598054f, 0.65328151f)).Using(comparer4));
            Assert.That((Vector3)s3, Is.EqualTo(new Vector3(-.99999994f, -.99999994f, -1)).Using(comparer3));
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
