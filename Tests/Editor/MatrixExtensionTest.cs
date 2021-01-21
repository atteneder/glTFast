// Copyright 2020-2021 Andreas Atteneder
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

namespace GLTFast.Tests
{
    public class MatrixExtensionTest {

        [Test]
        public void MatrixDecomposeTest() {
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
                m.m00,m.m01,m.m02,m.m03,
                m.m10,m.m11,m.m12,m.m13,
                m.m20,m.m21,m.m22,m.m23,
                m.m30,m.m31,m.m32,m.m33
            );

            for (int i = 0; i < 100000; i++) {
                Profiler.BeginSample("Matrix4x4.DecomposeUnity");
                if(m.ValidTRS()) {
                    Vector3 t1 = new Vector3( m.m03, m.m13, m.m23 );
                    Quaternion r1 = m.rotation;
                    Vector3 s1 = m.lossyScale;
                }
                Profiler.EndSample();

                Profiler.BeginSample("Matrix4x4.DecomposeCustom");
                m.Decompose(out var t, out var r, out var s);
                Profiler.EndSample();

                Profiler.BeginSample("float4x4.Decompose");
                m2.Decompose(out var t3, out var r3, out var s3);
                Profiler.EndSample();
            }
        }
    }
}