#if DEBUG
using System.Collections.Generic;
#endif

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace GLTFast
{
#if BURST
    using Unity.Mathematics;
#endif
    using Schema;

    struct VertexInputData {
        public byte[] buffer;
        public int startOffset;
        public int count;
        public int byteStride;
        public GLTFComponentType type;
    }

    abstract class VertexBufferConfigBase {
#if DEBUG
        public HashSet<int> meshIndices;
#endif
        protected VertexAttributeDescriptor[] vad;

        public abstract unsafe JobHandle? Init(VertexInputData posInput);
        public abstract void ApplyOnMesh(UnityEngine.Mesh msh);
        public abstract void Dispose();

        protected unsafe JobHandle? GetVector3sJob(
            void* input,
            int count,
            GLTFComponentType inputType,
            int inputByteStride,
            Vector3* output,
            bool normalized = false
        ) {
            JobHandle? jobHandle;

            UnityEngine.Profiling.Profiler.BeginSample("PrepareGetVector3sJob");
            if(inputType == GLTFComponentType.Float) {
                var job = new Jobs.GetVector3sInterleavedJob();
                job.byteStride = inputByteStride;
                job.input = (byte*)input;
                job.result = output;
                jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
            } else
            if(inputType == GLTFComponentType.UnsignedShort) {
                if (normalized) {
                    var job = new Jobs.GetUInt16PositionsInterleavedNormalizedJob();
                    job.byteStride = inputByteStride;
                    job.input = (byte*)input;
                    job.result = output;
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var job = new Jobs.GetUInt16PositionsInterleavedJob();
                    job.byteStride = inputByteStride;
                    job.input = (byte*)input;
                    job.result = output;
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                }
            } else
            if(inputType == GLTFComponentType.Short) {
                // TODO: test. did not have test files
                if (normalized) {
                    var job = new Jobs.GetVector3FromInt16InterleavedNormalizedJob();
                    job.byteStride = inputByteStride;
                    job.input = (byte*)input;
                    job.result = output;
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var job = new Jobs.GetVector3FromInt16InterleavedJob();
                    job.byteStride = inputByteStride;
                    job.input = (byte*)input;
                    job.result = output;
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                }
            } else
            if(inputType == GLTFComponentType.Byte) {
                // TODO: test positions. did not have test files
                if (normalized) {
                    var job = new Jobs.GetVector3FromSByteInterleavedNormalizedJob();
                    job.Setup(inputByteStride,(sbyte*)input,output);
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var job = new Jobs.GetVector3FromSByteInterleavedJob();
                    job.Setup(inputByteStride,(sbyte*)input,output);
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                }
            } else
            if(inputType == GLTFComponentType.UnsignedByte) {
                // TODO: test. did not have test files
                if (normalized) {
                    var job = new Jobs.GetVector3FromByteInterleavedNormalizedJob();
                    job.Setup(inputByteStride,(byte*)input,output);
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var job = new Jobs.GetVector3FromByteInterleavedJob();
                    job.Setup(inputByteStride,(byte*)input,output);
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                }
            } else {
                Debug.LogError("Unknown componentType");
                jobHandle = null;
            }
            UnityEngine.Profiling.Profiler.EndSample();
            return jobHandle;
        }
    }
}
