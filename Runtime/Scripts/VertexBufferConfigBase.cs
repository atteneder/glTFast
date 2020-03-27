#if DEBUG
using System.Collections.Generic;
#endif

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace GLTFast
{
#if BURST
    using Unity.Mathematics;
#endif
    using Schema;

    [System.Flags]
    enum MainBufferType {
        None = 0x0,
        Position = 0x1,
        Normal = 0x2,
        Tangent = 0x4,
        
        PosNorm = 0x3,
        PosNormTan = 0x7,
    }

    struct VertexInputData {

        public Accessor accessor;
        public BufferView bufferView;
        public int chunkStart;
        public byte[] buffer;

        public int startOffset {
            get { return accessor.byteOffset + bufferView.byteOffset + chunkStart; }
        }

        public int count {
            get { return accessor.count; }
        }

        public int byteStride {
            get { return bufferView.byteStride; }
        }

        public GLTFComponentType type {
            get { return accessor.componentType; }
        }

        public GLTFAccessorAttributeType attributeType {
            get { return accessor.typeEnum; }
        }
        public bool normalize {
            get { return accessor.normalized; }
        }
    }

    abstract class VertexBufferConfigBase {

        public bool calculateNormals = false;
        public bool calculateTangents = false;

        protected VertexAttributeDescriptor[] vad;

        public abstract unsafe JobHandle? ScheduleVertexJobs(
            VertexInputData posInput,
            VertexInputData? nrmInput = null,
            VertexInputData? tanInput = null,
            VertexInputData[] uvInputs = null,
            VertexInputData? colorInput = null
            );
        public abstract void ApplyOnMesh(UnityEngine.Mesh msh, MeshUpdateFlags flags = MeshUpdateFlags.Default);
        public abstract void Dispose();

        protected unsafe JobHandle? GetVector3sJob(
            void* input,
            int count,
            GLTFComponentType inputType,
            int inputByteStride,
            Vector3* output,
            int outputByteStride,
            bool normalized = false
        ) {
            JobHandle? jobHandle;

            Profiler.BeginSample("GetVector3sJob");
            if(inputType == GLTFComponentType.Float) {
                var job = new Jobs.GetVector3sInterleavedJob();
                job.inputByteStride = (inputByteStride>0) ? inputByteStride : 12;
                job.input = (byte*)input;
                job.outputByteStride = outputByteStride;
                job.result = output;
                jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
            } else
            if(inputType == GLTFComponentType.UnsignedShort) {
                if (normalized) {
                    var job = new Jobs.GetUInt16PositionsInterleavedNormalizedJob();
                    job.inputByteStride = (inputByteStride>0) ? inputByteStride : 6;
                    job.input = (byte*)input;
                    job.outputByteStride = outputByteStride;
                    job.result = output;
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var job = new Jobs.GetUInt16PositionsInterleavedJob();
                    job.inputByteStride = (inputByteStride>0) ? inputByteStride : 6;
                    job.input = (byte*)input;
                    job.outputByteStride = outputByteStride;
                    job.result = output;
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                }
            } else
            if(inputType == GLTFComponentType.Short) {
                // TODO: test. did not have test files
                if (normalized) {
                    var job = new Jobs.GetVector3FromInt16InterleavedNormalizedJob();
                    job.inputByteStride = (inputByteStride>0) ? inputByteStride : 6;
                    job.input = (byte*)input;
                    job.outputByteStride = outputByteStride;
                    job.result = output;
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var job = new Jobs.GetVector3FromInt16InterleavedJob();
                    job.inputByteStride = (inputByteStride>0) ? inputByteStride : 6;
                    job.input = (byte*)input;
                    job.outputByteStride = outputByteStride;
                    job.result = output;
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                }
            } else
            if(inputType == GLTFComponentType.Byte) {
                // TODO: test positions. did not have test files
                if (normalized) {
                    var job = new Jobs.GetVector3FromSByteInterleavedNormalizedJob();
                    job.Setup((inputByteStride>0) ? inputByteStride : 3, (sbyte*)input,outputByteStride,output);
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var job = new Jobs.GetVector3FromSByteInterleavedJob();
                    job.Setup((inputByteStride>0) ? inputByteStride : 3,(sbyte*)input,outputByteStride,output);
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                }
            } else
            if(inputType == GLTFComponentType.UnsignedByte) {
                // TODO: test. did not have test files
                if (normalized) {
                    var job = new Jobs.GetVector3FromByteInterleavedNormalizedJob();
                    job.Setup((inputByteStride>0) ? inputByteStride : 3,(byte*)input,outputByteStride,output);
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var job = new Jobs.GetVector3FromByteInterleavedJob();
                    job.Setup((inputByteStride>0) ? inputByteStride : 3,(byte*)input,outputByteStride,output);
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                }
            } else {
                Debug.LogError("Unknown componentType");
                jobHandle = null;
            }
            Profiler.EndSample();
            return jobHandle;
        }
        
        protected unsafe JobHandle? GetTangentsJob(
            void* input,
            int count,
            GLTFComponentType inputType,
            int inputByteStride,
            Vector4* output,
            int outputByteStride,
            bool normalized = false
            )
        {
            Profiler.BeginSample("GetTangentsJob");
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
                case GLTFComponentType.Short:
                    var jobTangent = new Jobs.GetVector4sInt16NormalizedInterleavedJob();
                    jobTangent.inputByteStride = inputByteStride>0 ? inputByteStride : 8;;
                    Assert.IsTrue(normalized);
                    jobTangent.input = (System.Int16*)input;
                    jobTangent.outputByteStride = outputByteStride;
                    jobTangent.result = output;
                    jobHandle = jobTangent.Schedule(count,GLTFast.DefaultBatchCount);
                    break;
                case GLTFComponentType.Byte:
                    var jobTangentByte = new Jobs.GetVector4sInt8NormalizedInterleavedJob();
                    jobTangentByte.inputByteStride = inputByteStride>0 ? inputByteStride : 4;
                    Assert.IsTrue(normalized);
                    jobTangentByte.input = (sbyte*)input;
                    jobTangentByte.outputByteStride = outputByteStride;
                    jobTangentByte.result = output;
                    jobHandle = jobTangentByte.Schedule(count,GLTFast.DefaultBatchCount);
                    break;
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
