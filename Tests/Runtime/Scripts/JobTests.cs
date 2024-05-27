// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Schema;
using GLTFast.Vertex;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Tests.Jobs
{

    [TestFixture]
    class Vector3Jobs
    {

        const int k_Length = 10;
        float3 m_NormalizedReference = new float3(-.5f, .5f, 0.707107f);
        float3 m_Reference = new float3(-1, 13, 42);

        NativeArray<float3> m_Input;
        NativeArray<ushort> m_InputUInt16;
        NativeArray<short> m_InputInt16;
        NativeArray<byte> m_InputUInt8;
        NativeArray<sbyte> m_InputInt8;
        NativeArray<float3> m_Output;

        [OneTimeSetUp]
        public void SetUpTest()
        {
            m_Input = new NativeArray<float3>(k_Length, Allocator.Persistent);
            m_InputUInt16 = new NativeArray<ushort>(k_Length * 3, Allocator.Persistent);
            m_InputInt16 = new NativeArray<short>(k_Length * 3, Allocator.Persistent);
            m_InputUInt8 = new NativeArray<byte>(k_Length * 3, Allocator.Persistent);
            m_InputInt8 = new NativeArray<sbyte>(k_Length * 3, Allocator.Persistent);
            m_Output = new NativeArray<float3>(k_Length, Allocator.Persistent);

            {
                m_Input[1] = new float3(.5f, .5f, 0.707107f);

                m_InputUInt8[3] = byte.MaxValue / 2;
                m_InputUInt8[4] = byte.MaxValue / 2;
                m_InputUInt8[5] = 180;

                m_InputInt8[3] = sbyte.MaxValue / 2;
                m_InputInt8[4] = sbyte.MaxValue / 2;
                m_InputInt8[5] = 90;

                m_InputUInt16[3] = ushort.MaxValue / 2;
                m_InputUInt16[4] = ushort.MaxValue / 2;
                m_InputUInt16[5] = (ushort)(0.707107f * ushort.MaxValue);

                m_InputInt16[3] = short.MaxValue / 2;
                m_InputInt16[4] = short.MaxValue / 2;
                m_InputInt16[5] = (short)(0.707107f * short.MaxValue);
            }

            {
                m_Input[2] = new float3(1, 13, 42);

                m_InputUInt8[6] = 1;
                m_InputUInt8[7] = 13;
                m_InputUInt8[8] = 42;

                m_InputInt8[6] = 1;
                m_InputInt8[7] = 13;
                m_InputInt8[8] = 42;

                m_InputUInt16[6] = 1;
                m_InputUInt16[7] = 13;
                m_InputUInt16[8] = 42;

                m_InputInt16[6] = 1;
                m_InputInt16[7] = 13;
                m_InputInt16[8] = 42;
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            m_Input.Dispose();
            m_InputUInt16.Dispose();
            m_InputInt16.Dispose();
            m_InputUInt8.Dispose();
            m_InputInt8.Dispose();
            m_Output.Dispose();
        }

        void CheckNormalizedResult(float epsilon = float.Epsilon)
        {
            const int i = 1;
            Utils.AssertNearOrEqual(m_NormalizedReference, m_Output[i], epsilon);
        }

        void CheckResult(float epsilon = float.Epsilon)
        {
            const int i = 2;
            Utils.AssertNearOrEqual(m_Reference, m_Output[i], epsilon);
        }

        [Test]
        public unsafe void ConvertVector3FloatToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertVector3FloatToFloatInterleavedJob
            {
                inputByteStride = 12,
                input = (byte*)m_Input.GetUnsafeReadOnlyPtr(),
                outputByteStride = 12,
                result = (float3*)m_Output.GetUnsafePtr()
            };
            job.RunBatch(m_Input.Length);

            CheckNormalizedResult();
            CheckResult();
        }

        [Test]
        public unsafe void ConvertVector3FloatToFloatJob()
        {
            var job = new GLTFast.Jobs.ConvertVector3FloatToFloatJob
            {
                input = (float3*)m_Input.GetUnsafeReadOnlyPtr(),
                result = (float3*)m_Output.GetUnsafePtr()
            };
            job.Run(m_Input.Length);
            CheckNormalizedResult();
            CheckResult();
        }

        [Test]
        public unsafe void ConvertVector3Int16ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertVector3Int16ToFloatInterleavedNormalizedJob
            {
                input = (byte*)m_InputInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 6,
                result = (float3*)m_Output.GetUnsafePtr(),
                outputByteStride = 12
            };
            job.RunBatch(m_Output.Length);
            CheckNormalizedResult(Constants.epsilonInt16);
        }

        [Test]
        public unsafe void ConvertVector3Int8ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertVector3Int8ToFloatInterleavedNormalizedJob
            {
                input = (sbyte*)m_InputInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 3,
                result = (float3*)m_Output.GetUnsafePtr(),
                outputByteStride = 12
            };
            job.RunBatch(m_Output.Length);
            CheckNormalizedResult(Constants.epsilonInt8);
        }

        [Test]
        public unsafe void ConvertPositionsUInt16ToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertPositionsUInt16ToFloatInterleavedJob
            {
                input = (byte*)m_InputUInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 6,
                result = (float3*)m_Output.GetUnsafePtr(),
                outputByteStride = 12
            };
            job.RunBatch(m_Output.Length);
            CheckResult();
        }

        [Test]
        public unsafe void ConvertPositionsUInt16ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertPositionsUInt16ToFloatInterleavedNormalizedJob
            {
                input = (byte*)m_InputUInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 6,
                result = (float3*)m_Output.GetUnsafePtr(),
                outputByteStride = 12
            };
            job.RunBatch(m_Output.Length);
            CheckNormalizedResult(Constants.epsilonUInt16);
        }

        [Test]
        public unsafe void ConvertPositionsInt16ToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertPositionsInt16ToFloatInterleavedJob
            {
                input = (byte*)m_InputInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 6,
                result = (float3*)m_Output.GetUnsafePtr(),
                outputByteStride = 12
            };
            job.RunBatch(m_Output.Length);
            CheckResult();
        }

        [Test]
        public unsafe void ConvertPositionsInt8ToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertPositionsInt8ToFloatInterleavedJob
            {
                input = (sbyte*)m_InputInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 3,
                result = (float3*)m_Output.GetUnsafePtr(),
                outputByteStride = 12
            };
            job.RunBatch(m_Output.Length);
            CheckResult();
        }

        [Test]
        public unsafe void ConvertPositionsUInt8ToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertPositionsUInt8ToFloatInterleavedJob
            {
                input = (byte*)m_InputUInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 3,
                result = (float3*)m_Output.GetUnsafePtr(),
                outputByteStride = 12
            };
            job.RunBatch(m_Output.Length);
            CheckResult();
        }

        [Test]
        public unsafe void ConvertPositionsUInt8ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertPositionsUInt8ToFloatInterleavedNormalizedJob
            {
                input = (byte*)m_InputUInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 3,
                result = (float3*)m_Output.GetUnsafePtr(),
                outputByteStride = 12
            };
            job.RunBatch(m_Output.Length);
            CheckNormalizedResult(Constants.epsilonUInt8);
        }

        [Test]
        public unsafe void ConvertNormalsInt16ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertNormalsInt16ToFloatInterleavedNormalizedJob
            {
                input = (byte*)m_InputInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 6,
                result = (float3*)m_Output.GetUnsafePtr(),
                outputByteStride = 12
            };
            job.RunBatch(m_Output.Length);
            CheckNormalizedResult(Constants.epsilonInt16);
        }

        [Test]
        public unsafe void ConvertNormalsInt8ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertNormalsInt8ToFloatInterleavedNormalizedJob
            {
                input = (sbyte*)m_InputInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 3,
                result = (float3*)m_Output.GetUnsafePtr(),
                outputByteStride = 12
            };
            job.RunBatch(m_Output.Length);
            CheckNormalizedResult(Constants.epsilonInt8);
        }
    }

    [TestFixture]
    class PositionSparseJobs
    {

        const int k_Length = 10;

        NativeArray<int> m_Indices;
        NativeArray<float3> m_Input;
        NativeArray<float3> m_Output;

        [OneTimeSetUp]
        public void SetUpTest()
        {
            m_Indices = new NativeArray<int>(k_Length, Allocator.Persistent);
            m_Input = new NativeArray<float3>(k_Length, Allocator.Persistent);
            m_Output = new NativeArray<float3>(k_Length * 2, Allocator.Persistent);

            for (int i = 0; i < k_Length; i++)
            {
                m_Indices[i] = i * 2;
                m_Input[i] = new float3(i, k_Length - 1, 42);
            }
        }

        void CheckResult()
        {
            var endIndex = math.min(k_Length, 10);
            for (int i = 0; i < endIndex; i++)
            {
                Utils.AssertNearOrEqual(new float3(-i, k_Length - 1, 42), m_Output[i * 2]);
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            m_Input.Dispose();
            m_Output.Dispose();
            m_Indices.Dispose();
        }

        [Test]
        public unsafe void ConvertPositionsSparseJob()
        {
            const GltfComponentType indexType = GltfComponentType.UnsignedInt;
            const GltfComponentType valueType = GltfComponentType.Float;
            const bool normalized = false;

            var job = new GLTFast.Jobs.ConvertVector3SparseJob
            {
                indexBuffer = m_Indices.GetUnsafeReadOnlyPtr(),
                indexConverter = GLTFast.Jobs.CachedFunction.GetIndexConverter(indexType),
                inputByteStride = 3 * Accessor.GetComponentTypeSize(valueType),
                input = m_Input.GetUnsafeReadOnlyPtr(),
                valueConverter = GLTFast.Jobs.CachedFunction.GetPositionConverter(valueType, normalized),
                outputByteStride = 12,
                result = (float3*)m_Output.GetUnsafePtr(),
            };
            job.Run(m_Indices.Length);
            CheckResult();
        }
    }

    [TestFixture]
    class UVJobs
    {
        const int k_UVLength = 10;
        float2 m_NormalizedReference = new float2(.5f, 0f);
        float2 m_Reference = new float2(13, -41);

        NativeArray<float2> m_UVInput;
        NativeArray<ushort> m_InputUInt16;
        NativeArray<short> m_InputInt16;
        NativeArray<byte> m_InputUInt8;
        NativeArray<sbyte> m_InputInt8;
        NativeArray<float2> m_UVOutput;

        [OneTimeSetUp]
        public void SetUpTest()
        {
            m_UVInput = new NativeArray<float2>(k_UVLength, Allocator.Persistent);
            m_InputUInt16 = new NativeArray<ushort>(k_UVLength * 2, Allocator.Persistent);
            m_InputInt16 = new NativeArray<short>(k_UVLength * 2, Allocator.Persistent);
            m_InputUInt8 = new NativeArray<byte>(k_UVLength * 2, Allocator.Persistent);
            m_InputInt8 = new NativeArray<sbyte>(k_UVLength * 2, Allocator.Persistent);
            m_UVOutput = new NativeArray<float2>(k_UVLength, Allocator.Persistent);

            {
                m_UVInput[1] = new float2(.5f, 1f);

                m_InputUInt8[2] = byte.MaxValue / 2;
                m_InputUInt8[3] = byte.MaxValue;

                m_InputInt8[2] = SByte.MaxValue / 2;
                m_InputInt8[3] = SByte.MaxValue;

                m_InputUInt16[2] = ushort.MaxValue / 2;
                m_InputUInt16[3] = ushort.MaxValue;

                m_InputInt16[2] = short.MaxValue / 2;
                m_InputInt16[3] = short.MaxValue;
            }

            {
                m_UVInput[2] = new float2(13, 42);

                m_InputUInt8[4] = 13;
                m_InputUInt8[5] = 42;

                m_InputInt8[4] = 13;
                m_InputInt8[5] = 42;

                m_InputUInt16[4] = 13;
                m_InputUInt16[5] = 42;

                m_InputInt16[4] = 13;
                m_InputInt16[5] = 42;
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            m_UVInput.Dispose();
            m_InputUInt16.Dispose();
            m_InputInt16.Dispose();
            m_InputUInt8.Dispose();
            m_InputInt8.Dispose();
            m_UVOutput.Dispose();
        }

        void CheckNormalizedResult(float epsilon = float.Epsilon)
        {
            const int i = 1;
            Utils.AssertNearOrEqual(m_NormalizedReference, m_UVOutput[i], epsilon);
        }

        void CheckResult(float epsilon = float.Epsilon)
        {
            const int i = 2;
            Utils.AssertNearOrEqual(m_Reference, m_UVOutput[i], epsilon);
        }

        // [Test]
        // public unsafe void ConvertUVsUInt8ToFloatJob() {
        //     var job = new GLTFast.Jobs.ConvertUVsUInt8ToFloatJob {
        //         input = (byte*)m_UVInput.GetUnsafeReadOnlyPtr(),
        //         result = (Vector2*)m_UVOutput.GetUnsafePtr()
        //     };
        //     job.Run(m_UVOutput.Length);
        // }
        //
        // [Test]
        // public unsafe void ConvertUVsUInt8ToFloatNormalizedJob() {
        //     var job = new GLTFast.Jobs.ConvertUVsUInt8ToFloatNormalizedJob {
        //         input = (byte*)m_UVInput.GetUnsafeReadOnlyPtr(),
        //         result = (Vector2*)m_UVOutput.GetUnsafePtr()
        //     };
        //     job.Run(m_UVOutput.Length);
        // }
        //
        // [Test]
        // public unsafe void ConvertUVsUInt16ToFloatNormalizedJob() {
        //     var job = new GLTFast.Jobs.ConvertUVsUInt16ToFloatNormalizedJob {
        //         input = (ushort*)m_UVInput.GetUnsafeReadOnlyPtr(),
        //         result = (Vector2*)m_UVOutput.GetUnsafePtr()
        //     };
        //     job.Run(m_UVOutput.Length);
        // }
        //
        // [Test]
        // public unsafe void ConvertUVsUInt16ToFloatJob() {
        //     var job = new GLTFast.Jobs.ConvertUVsUInt16ToFloatJob {
        //         input = (ushort*)m_UVInput.GetUnsafeReadOnlyPtr(),
        //         result = (Vector2*)m_UVOutput.GetUnsafePtr()
        //     };
        //     job.Run(m_UVOutput.Length);
        // }
        //
        // [Test]
        // public unsafe void ConvertUVsFloatToFloatJob() {
        //     var job = new GLTFast.Jobs.ConvertUVsFloatToFloatJob {
        //         input = (float*)m_UVInput.GetUnsafeReadOnlyPtr(),
        //         result = (Vector2*)m_UVOutput.GetUnsafePtr()
        //     };
        //     job.Run(m_UVOutput.Length);
        // }

        [Test]
        public unsafe void ConvertUVsUInt8ToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertUVsUInt8ToFloatInterleavedJob
            {
                input = (byte*)m_InputUInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 2,
                result = (float2*)m_UVOutput.GetUnsafePtr(),
                outputByteStride = 8
            };
            job.RunBatch(m_UVOutput.Length);
            CheckResult(Constants.epsilonUInt8);
        }

        [Test]
        public unsafe void ConvertUVsUInt8ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertUVsUInt8ToFloatInterleavedNormalizedJob
            {
                input = (byte*)m_InputUInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 2,
                result = (float2*)m_UVOutput.GetUnsafePtr(),
                outputByteStride = 8
            };
            job.Run(m_UVOutput.Length);
            CheckNormalizedResult(Constants.epsilonUInt8);
        }

        [Test]
        public unsafe void ConvertUVsUInt16ToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertUVsUInt16ToFloatInterleavedJob
            {
                input = (byte*)m_InputUInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 4,
                result = (float2*)m_UVOutput.GetUnsafePtr(),
                outputByteStride = 8
            };
            job.RunBatch(m_UVOutput.Length);
            CheckResult(Constants.epsilonUInt16);
        }

        [Test]
        public unsafe void ConvertUVsUInt16ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertUVsUInt16ToFloatInterleavedNormalizedJob
            {
                input = (byte*)m_InputUInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 4,
                result = (float2*)m_UVOutput.GetUnsafePtr(),
                outputByteStride = 8
            };
            job.Run(m_UVOutput.Length);
            CheckNormalizedResult(Constants.epsilonUInt16);
        }

        [Test]
        public unsafe void ConvertUVsInt16ToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertUVsInt16ToFloatInterleavedJob
            {
                input = (short*)m_InputInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 4,
                result = (float2*)m_UVOutput.GetUnsafePtr(),
                outputByteStride = 8
            };
            job.RunBatch(m_UVOutput.Length);
            CheckResult(Constants.epsilonInt16);
        }

        [Test]
        public unsafe void ConvertUVsInt16ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertUVsInt16ToFloatInterleavedNormalizedJob
            {
                input = (short*)m_InputInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 4,
                result = (float2*)m_UVOutput.GetUnsafePtr(),
                outputByteStride = 8
            };
            job.RunBatch(m_UVOutput.Length);
            CheckNormalizedResult(Constants.epsilonInt16);
        }

        [Test]
        public unsafe void ConvertUVsInt8ToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertUVsInt8ToFloatInterleavedJob
            {
                input = (sbyte*)m_InputInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 2,
                result = (float2*)m_UVOutput.GetUnsafePtr(),
                outputByteStride = 8
            };
            job.RunBatch(m_UVOutput.Length);
            CheckResult(Constants.epsilonInt8);
        }

        [Test]
        public unsafe void ConvertUVsInt8ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertUVsInt8ToFloatInterleavedNormalizedJob
            {
                input = (sbyte*)m_InputInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 2,
                result = (float2*)m_UVOutput.GetUnsafePtr(),
                outputByteStride = 8
            };
            job.RunBatch(m_UVOutput.Length);
            CheckNormalizedResult(Constants.epsilonInt8);
        }

        [Test]
        public unsafe void ConvertUVsFloatToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertUVsFloatToFloatInterleavedJob
            {
                input = (byte*)m_UVInput.GetUnsafeReadOnlyPtr(),
                inputByteStride = 8,
                result = (float2*)m_UVOutput.GetUnsafePtr(),
                outputByteStride = 8
            };
            job.RunBatch(m_UVOutput.Length);
            CheckResult();
            CheckNormalizedResult();
        }
    }

    [TestFixture]
    class Vector4Jobs
    {
        const int k_RotationLength = 10;
        float4 m_NormalizedReference = new float4(0.844623f, -0.191342f, -0.46194f, 0.191342f);
        float4 m_NormalizedTangentReference = new float4(0.844623f, 0.191342f, -0.46194f, 0.191342f);
        float4 m_RotationReference = new float4(2, -13, -42, 1);
        float4 m_TangentReference = new float4(2, 13, -42, 1);
        float4 m_Reference = new float4(2, 13, 42, 1);

        NativeArray<float4> m_RotInput;
        NativeArray<ushort> m_InputUInt16;
        NativeArray<short> m_InputInt16;
        NativeArray<byte> m_InputUInt8;
        NativeArray<sbyte> m_InputInt8;
        NativeArray<float4> m_RotOutput;

        [OneTimeSetUp]
        public void SetUpTest()
        {
            m_RotInput = new NativeArray<float4>(k_RotationLength, Allocator.Persistent);
            m_InputUInt16 = new NativeArray<ushort>(k_RotationLength * 4, Allocator.Persistent);
            m_InputInt16 = new NativeArray<short>(k_RotationLength * 4, Allocator.Persistent);
            m_InputUInt8 = new NativeArray<byte>(k_RotationLength * 4, Allocator.Persistent);
            m_InputInt8 = new NativeArray<sbyte>(k_RotationLength * 4, Allocator.Persistent);
            m_RotOutput = new NativeArray<float4>(k_RotationLength, Allocator.Persistent);

            {
                var tmp = new float4(0.844623f, 0.191342f, 0.46194f, 0.191342f);
                m_RotInput[1] = tmp;

                m_InputUInt8[4] = (byte)(byte.MaxValue * tmp.x);
                m_InputUInt8[5] = (byte)(byte.MaxValue * tmp.y);
                m_InputUInt8[6] = (byte)(byte.MaxValue * tmp.z);
                m_InputUInt8[7] = (byte)(byte.MaxValue * tmp.w);

                m_InputInt8[4] = (sbyte)(sbyte.MaxValue * tmp.x);
                m_InputInt8[5] = (sbyte)(sbyte.MaxValue * tmp.y);
                m_InputInt8[6] = (sbyte)(sbyte.MaxValue * tmp.z);
                m_InputInt8[7] = (sbyte)(sbyte.MaxValue * tmp.w);

                m_InputUInt16[4] = (ushort)(ushort.MaxValue * tmp.x);
                m_InputUInt16[5] = (ushort)(ushort.MaxValue * tmp.y);
                m_InputUInt16[6] = (ushort)(ushort.MaxValue * tmp.z);
                m_InputUInt16[7] = (ushort)(ushort.MaxValue * tmp.w);

                m_InputInt16[4] = (short)(short.MaxValue * tmp.x);
                m_InputInt16[5] = (short)(short.MaxValue * tmp.y);
                m_InputInt16[6] = (short)(short.MaxValue * tmp.z);
                m_InputInt16[7] = (short)(short.MaxValue * tmp.w);
            }

            {
                m_RotInput[2] = new float4(2, 13, 42, 1);

                m_InputUInt8[8] = 2;
                m_InputUInt8[9] = 13;
                m_InputUInt8[10] = 42;
                m_InputUInt8[11] = 1;

                m_InputInt8[8] = 2;
                m_InputInt8[9] = 13;
                m_InputInt8[10] = 42;
                m_InputInt8[11] = 1;

                m_InputUInt16[8] = 2;
                m_InputUInt16[9] = 13;
                m_InputUInt16[10] = 42;
                m_InputUInt16[11] = 1;

                m_InputInt16[8] = 2;
                m_InputInt16[9] = 13;
                m_InputInt16[10] = 42;
                m_InputInt16[11] = 1;
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            m_RotInput.Dispose();
            m_InputUInt16.Dispose();
            m_InputInt16.Dispose();
            m_InputUInt8.Dispose();
            m_InputInt8.Dispose();
            m_RotOutput.Dispose();
        }

        void CheckNormalizedResult(float epsilon = float.Epsilon)
        {
            const int i = 1;
            Utils.AssertNearOrEqual(m_NormalizedReference, m_RotOutput[i], epsilon);
        }

        void CheckNormalizedTangentResult(float epsilon = float.Epsilon)
        {
            const int i = 1;
            Utils.AssertNearOrEqual(m_NormalizedTangentReference, m_RotOutput[i], epsilon);
        }

        void CheckResult(float epsilon = float.Epsilon)
        {
            const int i = 2;
            Utils.AssertNearOrEqual(m_RotationReference, m_RotOutput[i], epsilon);
        }

        void CheckTangentResult(float epsilon = float.Epsilon)
        {
            const int i = 2;
            Utils.AssertNearOrEqual(m_TangentReference, m_RotOutput[i], epsilon);
        }

        void CheckBoneWeightResult(float epsilon = float.Epsilon)
        {
            const int i = 2;
            Utils.AssertNearOrEqual(m_Reference, m_RotOutput[i], epsilon);
        }

        [Test]
        public unsafe void ConvertRotationsFloatToFloatJob()
        {
            var job = new GLTFast.Jobs.ConvertRotationsFloatToFloatJob
            {
                input = (float4*)m_RotInput.GetUnsafeReadOnlyPtr(),
                result = (float4*)m_RotOutput.GetUnsafePtr()
            };
            job.Run(m_RotOutput.Length);
            CheckNormalizedResult();
            CheckResult();
        }

        [Test]
        public unsafe void ConvertRotationsInt16ToFloatJob()
        {
            var job = new GLTFast.Jobs.ConvertRotationsInt16ToFloatJob
            {
                input = (short*)m_InputInt16.GetUnsafeReadOnlyPtr(),
                result = (float*)m_RotOutput.GetUnsafePtr()
            };
            job.Run(m_RotOutput.Length);
            CheckNormalizedResult(Constants.epsilonInt16);
        }

        [Test]
        public unsafe void ConvertRotationsInt8ToFloatJob()
        {
            m_InputInt8[0] = sbyte.MinValue;
            m_InputInt8[1] = -64;
            m_InputInt8[2] = 64;
            m_InputInt8[3] = sbyte.MaxValue;

            var job = new GLTFast.Jobs.ConvertRotationsInt8ToFloatJob
            {
                input = (sbyte*)m_InputInt8.GetUnsafeReadOnlyPtr(),
                result = (float*)m_RotOutput.GetUnsafePtr()
            };
            job.Run(m_RotOutput.Length);
            Utils.AssertNearOrEqual(new float4(-1, .5f, -.5f, 1), m_RotOutput[0], Constants.epsilonInt8);
            CheckNormalizedResult(Constants.epsilonInt8);
        }

        [Test]
        public unsafe void ConvertTangentsFloatToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertTangentsFloatToFloatInterleavedJob
            {
                input = (byte*)m_RotInput.GetUnsafeReadOnlyPtr(),
                inputByteStride = 16,
                result = (float4*)m_RotOutput.GetUnsafePtr(),
                outputByteStride = 16
            };
            job.RunBatch(m_RotOutput.Length);
            CheckTangentResult();
        }

        [Test]
        public unsafe void ConvertBoneWeightsFloatToFloatInterleavedJob()
        {
            var job = new GLTFast.Jobs.ConvertBoneWeightsFloatToFloatInterleavedJob
            {
                input = (byte*)m_RotInput.GetUnsafeReadOnlyPtr(),
                inputByteStride = 16,
                result = (float4*)m_RotOutput.GetUnsafePtr(),
                outputByteStride = 16
            };
            job.RunBatch(m_RotOutput.Length);
            CheckBoneWeightResult();
        }

        // TODO: Test ConvertBoneWeightsUInt8ToFloatInterleavedJob
        // TODO: Test ConvertBoneWeightsUInt16ToFloatInterleavedJob

        [Test]
        public unsafe void ConvertTangentsInt16ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertTangentsInt16ToFloatInterleavedNormalizedJob
            {
                input = (short*)m_InputInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 8,
                result = (float4*)m_RotOutput.GetUnsafePtr(),
                outputByteStride = 16
            };
            job.RunBatch(m_RotOutput.Length);
            CheckNormalizedTangentResult(Constants.epsilonInt16);
        }

        [Test]
        public unsafe void ConvertTangentsInt8ToFloatInterleavedNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertTangentsInt8ToFloatInterleavedNormalizedJob
            {
                input = (sbyte*)m_InputInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 4,
                result = (float4*)m_RotOutput.GetUnsafePtr(),
                outputByteStride = 16
            };
            job.RunBatch(m_RotOutput.Length);
            CheckNormalizedTangentResult(Constants.epsilonInt8);
        }
    }

    [TestFixture]
    class ColorJobs
    {
        const int k_ColorLength = 10;
        Color m_ReferenceRGB = new Color(.13f, .42f, .95f, 1f);
        Color m_ReferenceRGBA = new Color(.42f, .95f, .5f, .24f);

        NativeArray<float> m_ColorInput;
        NativeArray<ushort> m_InputUInt16;
        NativeArray<byte> m_InputUInt8;
        NativeArray<float4> m_ColorOutput;

        [OneTimeSetUp]
        public void SetUpTest()
        {
            m_ColorInput = new NativeArray<float>(k_ColorLength * 4, Allocator.Persistent);
            m_InputUInt16 = new NativeArray<ushort>(k_ColorLength * 4, Allocator.Persistent);
            m_InputUInt8 = new NativeArray<byte>(k_ColorLength * 4, Allocator.Persistent);
            m_ColorOutput = new NativeArray<float4>(k_ColorLength, Allocator.Persistent);

            m_ColorInput[3] = m_ReferenceRGB.r;
            m_ColorInput[4] = m_ReferenceRGB.g;
            m_ColorInput[5] = m_ReferenceRGB.b;
            m_ColorInput[6] = m_ReferenceRGBA.b;
            m_ColorInput[7] = m_ReferenceRGBA.a;

            m_InputUInt8[3] = (byte)(byte.MaxValue * m_ReferenceRGB.r);
            m_InputUInt8[4] = (byte)(byte.MaxValue * m_ReferenceRGB.g);
            m_InputUInt8[5] = (byte)(byte.MaxValue * m_ReferenceRGB.b);
            m_InputUInt8[6] = (byte)(byte.MaxValue * m_ReferenceRGBA.b);
            m_InputUInt8[7] = (byte)(byte.MaxValue * m_ReferenceRGBA.a);

            m_InputUInt16[3] = (ushort)(ushort.MaxValue * m_ReferenceRGB.r);
            m_InputUInt16[4] = (ushort)(ushort.MaxValue * m_ReferenceRGB.g);
            m_InputUInt16[5] = (ushort)(ushort.MaxValue * m_ReferenceRGB.b);
            m_InputUInt16[6] = (ushort)(ushort.MaxValue * m_ReferenceRGBA.b);
            m_InputUInt16[7] = (ushort)(ushort.MaxValue * m_ReferenceRGBA.a);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            m_ColorInput.Dispose();
            m_InputUInt8.Dispose();
            m_InputUInt16.Dispose();
            m_ColorOutput.Dispose();
        }

        void CheckResultRGB(float epsilon = float.Epsilon)
        {
            const int i = 1;
            Utils.AssertNearOrEqual(m_ReferenceRGB, m_ColorOutput[i], epsilon);
        }

        void CheckResultRGBA(float epsilon = float.Epsilon)
        {
            const int i = 1;
            Utils.AssertNearOrEqual(m_ReferenceRGBA, m_ColorOutput[i], epsilon);
        }

        [Test]
        public unsafe void ConvertColorsRGBFloatToRGBAFloatJob()
        {
            var job = new GLTFast.Jobs.ConvertColorsRGBFloatToRGBAFloatJob
            {
                input = (byte*)m_ColorInput.GetUnsafeReadOnlyPtr(),
                inputByteStride = 12,
                result = (float4*)m_ColorOutput.GetUnsafePtr()
            };
            job.Run(m_ColorOutput.Length);
            CheckResultRGB();
        }

        [Test]
        public unsafe void ConvertColorsRGBUInt8ToRGBAFloatJob()
        {
            var job = new GLTFast.Jobs.ConvertColorsRgbUInt8ToRGBAFloatJob
            {
                input = (byte*)m_InputUInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 3,
                result = m_ColorOutput
            };
            job.Run(m_ColorOutput.Length);
            CheckResultRGB(Constants.epsilonUInt8);
        }

        [Test]
        public unsafe void ConvertColorsRGBUInt16ToRGBAFloatJob()
        {
            var job = new GLTFast.Jobs.ConvertColorsRgbUInt16ToRGBAFloatJob
            {
                input = (ushort*)m_InputUInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 6,
                result = m_ColorOutput
            };
            job.Run(m_ColorOutput.Length);
            CheckResultRGB(Constants.epsilonUInt16);
        }

        [Test]
        public unsafe void ConvertColorsRGBAUInt16ToRGBAFloatJob()
        {
            var job = new GLTFast.Jobs.ConvertColorsRgbaUInt16ToRGBAFloatJob
            {
                input = (ushort*)m_InputUInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 8,
                result = (float4*)m_ColorOutput.GetUnsafePtr()
            };
            job.RunBatch(m_ColorOutput.Length);
            CheckResultRGBA(Constants.epsilonUInt16);
        }

        [Test]
        public unsafe void ConvertColorsRGBAFloatToRGBAFloatJob()
        {
            var job = new GLTFast.Jobs.ConvertColorsRGBAFloatToRGBAFloatJob
            {
                input = (byte*)m_ColorInput.GetUnsafeReadOnlyPtr(),
                inputByteStride = 16,
                result = (float4*)m_ColorOutput.GetUnsafePtr()
            };
            job.RunBatch(m_ColorOutput.Length);
            CheckResultRGBA(Constants.epsilonUInt16);
        }

        [Test]
        public unsafe void ConvertColorsRGBAUInt8ToRGBAFloatJob()
        {
            var job = new GLTFast.Jobs.ConvertColorsRgbaUInt8ToRGBAFloatJob
            {
                input = (byte*)m_InputUInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 4,
                result = m_ColorOutput
            };
            job.Run(m_ColorOutput.Length);
            CheckResultRGBA(Constants.epsilonUInt8);
        }
    }

    [TestFixture]
    class BoneIndexJobs
    {
        const int k_BoneIndexLength = 10;
        uint4 m_Reference = new uint4(2, 3, 4, 5);
        NativeArray<byte> m_InputUInt8;
        NativeArray<ushort> m_InputUInt16;
        NativeArray<uint4> m_BoneIndexOutput;

        [OneTimeSetUp]
        public void SetUpTest()
        {
            m_InputUInt8 = new NativeArray<byte>(k_BoneIndexLength * 4, Allocator.Persistent);
            m_InputUInt16 = new NativeArray<ushort>(k_BoneIndexLength * 4, Allocator.Persistent);
            m_BoneIndexOutput = new NativeArray<uint4>(k_BoneIndexLength, Allocator.Persistent);

            m_InputUInt8[4] = (byte)m_Reference.x;
            m_InputUInt8[5] = (byte)m_Reference.y;
            m_InputUInt8[6] = (byte)m_Reference.z;
            m_InputUInt8[7] = (byte)m_Reference.w;

            m_InputUInt16[4] = (ushort)m_Reference.x;
            m_InputUInt16[5] = (ushort)m_Reference.y;
            m_InputUInt16[6] = (ushort)m_Reference.z;
            m_InputUInt16[7] = (ushort)m_Reference.w;
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            m_InputUInt16.Dispose();
            m_InputUInt8.Dispose();
            m_BoneIndexOutput.Dispose();
        }

        void CheckResult()
        {
            const int i = 1;
            Utils.AssertNearOrEqual(m_Reference, m_BoneIndexOutput[i]);
        }

        [Test]
        public unsafe void ConvertBoneJointsUInt8ToUInt32Job()
        {
            var job = new GLTFast.Jobs.ConvertBoneJointsUInt8ToUInt32Job
            {
                input = (byte*)m_InputUInt8.GetUnsafeReadOnlyPtr(),
                inputByteStride = 4,
                result = (uint4*)m_BoneIndexOutput.GetUnsafePtr(),
                outputByteStride = 16
            };
            job.Run(m_BoneIndexOutput.Length);
            CheckResult();
        }

        [Test]
        public unsafe void ConvertBoneJointsUInt16ToUInt32Job()
        {
            var job = new GLTFast.Jobs.ConvertBoneJointsUInt16ToUInt32Job
            {
                input = (byte*)m_InputUInt16.GetUnsafeReadOnlyPtr(),
                inputByteStride = 8,
                result = (uint4*)m_BoneIndexOutput.GetUnsafePtr(),
                outputByteStride = 16
            };
            job.Run(m_BoneIndexOutput.Length);
            CheckResult();
        }
    }

    [TestFixture]
    class SortJointsJobs
    {

        const int k_BoneIndexLength = 24;

        NativeArray<VBones> m_Input;

        [OneTimeSetUp]
        public unsafe void SetUpTest()
        {
            m_Input = new NativeArray<VBones>(k_BoneIndexLength, Allocator.Persistent);
            var v = new VBones();
            var index = 0;
            foreach (var i in AllFour())
            {
                v.joints[i[0]] = 0;
                v.joints[i[1]] = 1;
                v.joints[i[2]] = 2;
                v.joints[i[3]] = 3;

                v.weights[i[0]] = .5f;
                v.weights[i[1]] = .25f;
                v.weights[i[2]] = .20f;
                v.weights[i[3]] = .05f;
                m_Input[index] = v;
                index++;
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            m_Input.Dispose();
        }

        unsafe void CheckResult()
        {
            foreach (var v in m_Input)
            {
                Assert.AreEqual(0, v.joints[0]);
                Assert.AreEqual(1, v.joints[1]);
                Assert.AreEqual(2, v.joints[2]);
                Assert.AreEqual(3, v.joints[3]);
                Assert.AreEqual(0.5f, v.weights[0]);
                Assert.AreEqual(.25f, v.weights[1]);
                Assert.AreEqual(.2f, v.weights[2]);
                Assert.AreEqual(.05f, v.weights[3]);
            }
        }

        [Test]
        public unsafe void SortAndRenormalizeBoneWeightsJob()
        {
            var job = new GLTFast.Jobs.SortAndNormalizeBoneWeightsJob()
            {
                bones = m_Input,
                skinWeights = 4
            };
            job.Run(m_Input.Length);
            CheckResult();
        }

        public static System.Collections.Generic.IEnumerable<int[]> AllFour()
        {
            yield return new[] { 0, 1, 2, 3 };
            yield return new[] { 0, 1, 3, 2 };
            yield return new[] { 0, 2, 3, 1 };
            yield return new[] { 0, 2, 1, 3 };
            yield return new[] { 0, 3, 1, 2 };
            yield return new[] { 0, 3, 2, 1 };
            yield return new[] { 1, 3, 2, 0 };
            yield return new[] { 1, 3, 0, 2 };
            yield return new[] { 1, 2, 0, 3 };
            yield return new[] { 1, 2, 3, 0 };
            yield return new[] { 1, 0, 3, 2 };
            yield return new[] { 1, 0, 2, 3 };
            yield return new[] { 2, 0, 1, 3 };
            yield return new[] { 2, 0, 3, 1 };
            yield return new[] { 2, 1, 0, 3 };
            yield return new[] { 2, 1, 3, 0 };
            yield return new[] { 2, 3, 0, 1 };
            yield return new[] { 2, 3, 1, 0 };
            yield return new[] { 3, 0, 2, 1 };
            yield return new[] { 3, 0, 1, 2 };
            yield return new[] { 3, 2, 1, 0 };
            yield return new[] { 3, 2, 0, 1 };
            yield return new[] { 3, 1, 0, 2 };
            yield return new[] { 3, 1, 2, 0 };
        }
    }

    [TestFixture]
    class MatrixJobs
    {
        const int k_MatrixLength = 10;
        static readonly Matrix4x4 k_Reference = new Matrix4x4(
            new Vector4(1, -5, -9, 13),
            new Vector4(-2, 6, 10, 14),
            new Vector4(-3, 7, 11, 15),
            new Vector4(-4, 8, 12, 16)
        );
        NativeArray<float4x4> m_MatrixInput;
        NativeArray<Matrix4x4> m_MatrixOutput;

        [OneTimeSetUp]
        public void SetUpTest()
        {
            m_MatrixInput = new NativeArray<float4x4>(k_MatrixLength, Allocator.Persistent);
            m_MatrixOutput = new NativeArray<Matrix4x4>(k_MatrixLength, Allocator.Persistent);

            m_MatrixInput[1] = new float4x4(
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 10, 11, 12,
                13, 14, 15, 16
                );
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            m_MatrixInput.Dispose();
            m_MatrixOutput.Dispose();
        }

        [Test]
        public unsafe void ConvertMatricesJob()
        {
            var job = new GLTFast.Jobs.ConvertMatricesJob
            {
                input = (float4x4*)m_MatrixInput.GetUnsafeReadOnlyPtr(),
                result = (float4x4*)m_MatrixOutput.GetUnsafePtr(),
            };
            job.Run(m_MatrixOutput.Length);
            Assert.AreEqual(k_Reference, m_MatrixOutput[1]);
        }
    }

    [TestFixture]
    class IndexJobs
    {
        const int k_IndexLength = 12; // multiple of 3!
        NativeArray<byte> m_InputUInt8;
        NativeArray<ushort> m_InputUInt16;
        NativeArray<uint> m_InputUInt32;
        NativeArray<int> m_IndexOutput;

        [OneTimeSetUp]
        public void SetUpTest()
        {
            m_InputUInt8 = new NativeArray<byte>(k_IndexLength, Allocator.Persistent);
            m_InputUInt16 = new NativeArray<ushort>(k_IndexLength, Allocator.Persistent);
            m_InputUInt32 = new NativeArray<uint>(k_IndexLength, Allocator.Persistent);
            m_IndexOutput = new NativeArray<int>(k_IndexLength, Allocator.Persistent);

            for (var i = 0; i < 6; i++)
            {
                m_InputUInt8[i] = (byte)i;
                m_InputUInt16[i] = (ushort)i;
                m_InputUInt32[i] = (uint)i;
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            m_InputUInt8.Dispose();
            m_InputUInt16.Dispose();
            m_InputUInt32.Dispose();
            m_IndexOutput.Dispose();
        }

        void CheckResult()
        {
            for (var i = 0; i < 6; i++)
            {
                Assert.AreEqual(i, m_IndexOutput[i]);
            }
        }

        void CheckResultFlipped()
        {
            Assert.AreEqual(0, m_IndexOutput[0]);
            Assert.AreEqual(2, m_IndexOutput[1]);
            Assert.AreEqual(1, m_IndexOutput[2]);
            Assert.AreEqual(3, m_IndexOutput[3]);
            Assert.AreEqual(5, m_IndexOutput[4]);
            Assert.AreEqual(4, m_IndexOutput[5]);
        }

        [Test]
        public unsafe void CreateIndicesInt32Job()
        {
            Assert.IsTrue(m_IndexOutput.Length % 3 == 0);
            var job = new GLTFast.Jobs.CreateIndicesInt32Job
            {
                result = (int*)m_IndexOutput.GetUnsafePtr()
            };
            job.Run(m_IndexOutput.Length);
            CheckResult();
        }

        [Test]
        public unsafe void CreateIndicesInt32FlippedJob()
        {
            Assert.IsTrue(m_IndexOutput.Length % 3 == 0);
            var job = new GLTFast.Jobs.CreateIndicesInt32FlippedJob
            {
                result = (int*)m_IndexOutput.GetUnsafePtr()
            };
            job.Run(m_IndexOutput.Length);
            Assert.AreEqual(2, m_IndexOutput[0]);
            Assert.AreEqual(1, m_IndexOutput[1]);
            Assert.AreEqual(0, m_IndexOutput[2]);
            Assert.AreEqual(5, m_IndexOutput[3]);
            Assert.AreEqual(4, m_IndexOutput[4]);
            Assert.AreEqual(3, m_IndexOutput[5]);
        }

        [Test]
        public unsafe void ConvertIndicesUInt8ToInt32Job()
        {
            Assert.IsTrue(m_IndexOutput.Length % 3 == 0);
            var job = new GLTFast.Jobs.ConvertIndicesUInt8ToInt32Job
            {
                input = (byte*)m_InputUInt8.GetUnsafeReadOnlyPtr(),
                result = (int*)m_IndexOutput.GetUnsafePtr()
            };
            job.Run(m_IndexOutput.Length);
            CheckResult();
        }

        [Test]
        public unsafe void ConvertIndicesUInt8ToInt32FlippedJob()
        {
            Assert.IsTrue(m_IndexOutput.Length % 3 == 0);
            var job = new GLTFast.Jobs.ConvertIndicesUInt8ToInt32FlippedJob
            {
                input = (byte*)m_InputUInt8.GetUnsafeReadOnlyPtr(),
                result = (int3*)m_IndexOutput.GetUnsafePtr()
            };
            job.Run(m_IndexOutput.Length / 3);
            CheckResultFlipped();
        }

        [Test]
        public unsafe void ConvertIndicesUInt16ToInt32FlippedJob()
        {
            Assert.IsTrue(m_IndexOutput.Length % 3 == 0);
            var job = new GLTFast.Jobs.ConvertIndicesUInt16ToInt32FlippedJob
            {
                input = (ushort*)m_InputUInt16.GetUnsafeReadOnlyPtr(),
                result = (int3*)m_IndexOutput.GetUnsafePtr()
            };
            job.Run(m_IndexOutput.Length / 3);
            CheckResultFlipped();
        }

        [Test]
        public unsafe void ConvertIndicesUInt16ToInt32Job()
        {
            Assert.IsTrue(m_IndexOutput.Length % 3 == 0);
            var job = new GLTFast.Jobs.ConvertIndicesUInt16ToInt32Job
            {
                input = (ushort*)m_InputUInt16.GetUnsafeReadOnlyPtr(),
                result = (int*)m_IndexOutput.GetUnsafePtr()
            };
            job.Run(m_IndexOutput.Length);
            CheckResult();
        }

        [Test]
        public unsafe void ConvertIndicesUInt32ToInt32Job()
        {
            Assert.IsTrue(m_IndexOutput.Length % 3 == 0);
            var job = new GLTFast.Jobs.ConvertIndicesUInt32ToInt32Job
            {
                input = (uint*)m_InputUInt32.GetUnsafeReadOnlyPtr(),
                result = (int*)m_IndexOutput.GetUnsafePtr()
            };
            job.Run(m_IndexOutput.Length);
            CheckResult();
        }

        [Test]
        public unsafe void ConvertIndicesUInt32ToInt32FlippedJob()
        {
            Assert.IsTrue(m_IndexOutput.Length % 3 == 0);
            var job = new GLTFast.Jobs.ConvertIndicesUInt32ToInt32FlippedJob
            {
                input = (uint*)m_InputUInt32.GetUnsafeReadOnlyPtr(),
                result = (int3*)m_IndexOutput.GetUnsafePtr()
            };
            job.Run(m_IndexOutput.Length / 3);
            CheckResultFlipped();
        }
    }

    [TestFixture]
    class ScalarJobs
    {
        const int k_ScalarLength = 10;
        NativeArray<sbyte> m_InputInt8;
        NativeArray<byte> m_InputUInt8;
        NativeArray<short> m_InputInt16;
        NativeArray<ushort> m_InputUInt16;
        NativeArray<float> m_ScalarOutput;

        [OneTimeSetUp]
        public void SetUpTest()
        {
            m_InputInt8 = new NativeArray<sbyte>(k_ScalarLength, Allocator.Persistent);
            m_InputUInt8 = new NativeArray<byte>(k_ScalarLength, Allocator.Persistent);
            m_InputInt16 = new NativeArray<short>(k_ScalarLength, Allocator.Persistent);
            m_InputUInt16 = new NativeArray<ushort>(k_ScalarLength, Allocator.Persistent);
            m_ScalarOutput = new NativeArray<float>(k_ScalarLength, Allocator.Persistent);

            m_InputInt8[0] = sbyte.MaxValue;
            m_InputUInt8[0] = byte.MaxValue;
            m_InputInt16[0] = short.MaxValue;
            m_InputUInt16[0] = ushort.MaxValue;

            m_InputInt8[1] = 0;
            m_InputUInt8[1] = 0;
            m_InputInt16[1] = 0;
            m_InputUInt16[1] = 0;

            m_InputInt8[2] = sbyte.MinValue;
            m_InputUInt8[2] = byte.MinValue;
            m_InputInt16[2] = short.MinValue;
            m_InputUInt16[2] = ushort.MinValue;

            m_InputInt8[3] = sbyte.MaxValue / 2;
            m_InputUInt8[3] = byte.MaxValue / 2;
            m_InputInt16[3] = short.MaxValue / 2;
            m_InputUInt16[3] = ushort.MaxValue / 2;
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            m_InputInt8.Dispose();
            m_InputUInt8.Dispose();
            m_InputInt16.Dispose();
            m_InputUInt16.Dispose();
            m_ScalarOutput.Dispose();
        }

        void CheckResult(float epsilon = float.Epsilon)
        {
            Utils.AssertNearOrEqual(1, m_ScalarOutput[0], epsilon);
            Utils.AssertNearOrEqual(0, m_ScalarOutput[1], epsilon);
            Utils.AssertNearOrEqual(0, m_ScalarOutput[2], epsilon);
            Utils.AssertNearOrEqual(.5f, m_ScalarOutput[3], epsilon);
        }

        void CheckSignedResult(float epsilon = float.Epsilon)
        {
            Utils.AssertNearOrEqual(1, m_ScalarOutput[0], epsilon);
            Utils.AssertNearOrEqual(0, m_ScalarOutput[1], epsilon);
            Utils.AssertNearOrEqual(-1, m_ScalarOutput[2], epsilon);
            Utils.AssertNearOrEqual(.5f, m_ScalarOutput[3], epsilon);
        }

        [Test]
        public unsafe void ConvertScalarInt8ToFloatNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertScalarInt8ToFloatNormalizedJob
            {
                input = (sbyte*)m_InputInt8.GetUnsafeReadOnlyPtr(),
                result = m_ScalarOutput,
            };
            job.Run(m_ScalarOutput.Length);
            CheckSignedResult(Constants.epsilonInt8);
        }

        [Test]
        public unsafe void ConvertScalarUInt8ToFloatNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertScalarUInt8ToFloatNormalizedJob
            {
                input = (byte*)m_InputUInt8.GetUnsafeReadOnlyPtr(),
                result = m_ScalarOutput,
            };
            job.Run(m_ScalarOutput.Length);
            CheckResult(Constants.epsilonUInt8);
        }

        [Test]
        public unsafe void ConvertScalarInt16ToFloatNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertScalarInt16ToFloatNormalizedJob
            {
                input = (short*)m_InputInt16.GetUnsafeReadOnlyPtr(),
                result = m_ScalarOutput,
            };
            job.Run(m_ScalarOutput.Length);
            CheckSignedResult(Constants.epsilonInt16);
        }

        [Test]
        public unsafe void ConvertScalarUInt16ToFloatNormalizedJob()
        {
            var job = new GLTFast.Jobs.ConvertScalarUInt16ToFloatNormalizedJob
            {
                input = (ushort*)m_InputUInt16.GetUnsafeReadOnlyPtr(),
                result = m_ScalarOutput,
            };
            job.Run(m_ScalarOutput.Length);
            CheckResult(Constants.epsilonUInt16);
        }
    }
}
