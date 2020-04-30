using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace GLTFast {

    using Vertex;
    using Schema;

    abstract class VertexBufferBonesBase {
        public abstract unsafe bool ScheduleVertexBonesJob(
            VertexInputData weightsInput,
            VertexInputData jointsInput,
            NativeSlice<JobHandle> handles
            );
        public abstract void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream);
        public abstract void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = MeshUpdateFlags.Default);
        public abstract void Dispose();
    }

    class VertexBufferBones : VertexBufferBonesBase {
        NativeArray<VBones> vData;

        public override unsafe bool ScheduleVertexBonesJob(
            VertexInputData weightsInput,
            VertexInputData jointsInput,
            NativeSlice<JobHandle> handles
            ) {
            Profiler.BeginSample("ScheduleVertexBonesJob");
            Profiler.BeginSample("AllocateNativeArray");
            vData = new NativeArray<VBones>(weightsInput.count, VertexBufferConfigBase.defaultAllocator);
            var vDataPtr = (byte*) NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(vData);
            Profiler.EndSample();

            fixed( void* input = &(weightsInput.buffer[weightsInput.startOffset])) {
                var h = GetWeightsJob(
                    input,
                    weightsInput.count,
                    weightsInput.type,
                    weightsInput.byteStride,
                    (Vector4*)vDataPtr,
                    32,
                    weightsInput.normalize
                );
                if (h.HasValue) {
                    handles[0] = h.Value;
                } else {
                    Profiler.EndSample();
                    return false;
                }
            }

            fixed( void* input = &(jointsInput.buffer[jointsInput.startOffset])) {
                var h = GetJointsJob(
                    input,
                    jointsInput.count,
                    jointsInput.type,
                    jointsInput.byteStride,
                    (uint*)(vDataPtr+16),
                    32,
                    jointsInput.normalize
                );
                if (h.HasValue) {
                    handles[1] = h.Value;
                } else {
                    Profiler.EndSample();
                    return false;
                }
            }
            Profiler.EndSample();
            return true;
        }

        public override void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream) {
            dst[offset] = new VertexAttributeDescriptor(VertexAttribute.BlendWeight, VertexAttributeFormat.Float32, 4, stream);
            dst[offset+1] = new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt32, 4, stream);
        }

        public override void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = MeshUpdateFlags.Default) {
            Profiler.BeginSample("ApplyBones");
            msh.SetVertexBufferData(vData,0,0,vData.Length,stream,flags);
            Profiler.EndSample();
        }

        public override void Dispose() {
            if (vData.IsCreated) {
                vData.Dispose();
            }
        }

        protected unsafe JobHandle? GetWeightsJob(
            void* input,
            int count,
            GLTFComponentType inputType,
            int inputByteStride,
            Vector4* output,
            int outputByteStride,
            bool normalized = false
            )
        {
            Profiler.BeginSample("GetWeightsJob");
            JobHandle? jobHandle;
            switch(inputType) {
                case GLTFComponentType.Float:
                    var jobTangentI = new Jobs.GetVector4sInterleavedJob();
                    jobTangentI.inputByteStride = inputByteStride>0 ? inputByteStride : 16;
                    jobTangentI.input = (byte*)input;
                    jobTangentI.outputByteStride = outputByteStride;
                    jobTangentI.result = output;
                    jobHandle = jobTangentI.Schedule(count,GLTFast.DefaultBatchCount);
                    break;
                // TODO: Complete those cases
                // case GLTFComponentType.UnsignedShort:
                //     break;
                // case GLTFComponentType.UnsignedByte:
                //     break;
                default:
                    Debug.LogErrorFormat( GLTFast.ErrorUnsupportedType, "Tangent", inputType);
                    jobHandle = null;
                    break;
            }

            Profiler.EndSample();
            return jobHandle;
        }

        protected unsafe JobHandle? GetJointsJob(
            void* input,
            int count,
            GLTFComponentType inputType,
            int inputByteStride,
            uint* output,
            int outputByteStride,
            bool normalized = false
            )
        {
            Profiler.BeginSample("GetJointsJob");
            JobHandle? jobHandle;
            switch(inputType) {
                // TODO: Complete
                // case GLTFComponentType.Float:
                //     break;
                case GLTFComponentType.UnsignedShort:
                    var jobTangent = new Jobs.GetJointsUInt16Job();
                    jobTangent.inputByteStride = inputByteStride>0 ? inputByteStride : 8;
                    // Assert.IsTrue(normalized);
                    jobTangent.input = (byte*)input;
                    jobTangent.outputByteStride = outputByteStride;
                    jobTangent.result = output;
                    jobHandle = jobTangent.Schedule(count,GLTFast.DefaultBatchCount);
                    break;
                // TODO: Complete
                // case GLTFComponentType.UnsignedByte:
                //     break;
                default:
                    Debug.LogErrorFormat( GLTFast.ErrorUnsupportedType, "Tangent", inputType);
                    jobHandle = null;
                    break;
            }

            Profiler.EndSample();
            return jobHandle;
        }
    }
}
