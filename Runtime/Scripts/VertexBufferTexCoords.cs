using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace GLTFast {

    using Vertex;
    using Schema;

    abstract class VertexBufferTexCoordsBase {
        public int uvSetCount { get; protected set; }
        public abstract unsafe bool ScheduleVertexUVJobs(VertexInputData[] uvInputs, NativeSlice<JobHandle> handles);
        public abstract void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream);
        public abstract void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = MeshUpdateFlags.Default);
        public abstract void Dispose();
    }

    class VertexBufferTexCoords<T> : VertexBufferTexCoordsBase where T : struct {
        NativeArray<T> vData;

        public override unsafe bool ScheduleVertexUVJobs(VertexInputData[] uvInputs, NativeSlice<JobHandle> handles) {
            Profiler.BeginSample("ScheduleVertexUVJobs");
            vData = new NativeArray<T>(uvInputs[0].count, Allocator.Persistent);
            var vDataPtr = (byte*) NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(vData);
            uvSetCount = uvInputs.Length;
            int outputByteStride = uvInputs.Length * 8;

            for (int i=0; i<uvInputs.Length; i++) {
                var uvInput = uvInputs[i];
                fixed( void* input = &(uvInput.buffer[uvInput.startOffset])) {
                    var h = GetUvsJob(
                        input,
                        uvInput.count,
                        uvInput.type,
                        uvInput.byteStride,
                        (Vector2*) (vDataPtr+(i*8)),
                        outputByteStride,
                        uvInput.normalize
                    );
                    if (h.HasValue) {
                        handles[i] = h.Value;
                    } else {
                        Profiler.EndSample();
                        return false;
                    }
                }
            }
            Profiler.EndSample();
            return true;
        }

        public override void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream) {
            VertexAttribute vatt = VertexAttribute.TexCoord0;
            for (int i = 0; i < uvSetCount; i++) {
                if (i == 1) {
                    vatt = VertexAttribute.TexCoord1;
                }
                dst[offset+i] = new VertexAttributeDescriptor(vatt, VertexAttributeFormat.Float32, 2, stream);
            }
        }

        public override void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = MeshUpdateFlags.Default) {
            Profiler.BeginSample("ApplyUVs");
            msh.SetVertexBufferData(vData,0,0,vData.Length,stream,flags);
            Profiler.EndSample();
        }

        public override void Dispose() {
            if (vData.IsCreated) {
                vData.Dispose();
            }
        }

        unsafe JobHandle? GetUvsJob(
            void* input,
            int count,
            GLTFComponentType inputType,
            int inputByteStride,
            Vector2* output,
            int outputByteStride,
            bool normalized = false
            )
        {
            Profiler.BeginSample("PrepareUVs");
            JobHandle? jobHandle = null;
            
            switch( inputType ) { 
            case GLTFComponentType.Float:
                {
                    var jobUv = new Jobs.GetVector2sInterleavedJob {
                        inputByteStride = (inputByteStride>0) ? inputByteStride : 8,
                        input = (byte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobUv.Schedule(count,GLTFast.DefaultBatchCount);
                }
                break;
            case GLTFComponentType.UnsignedByte:
                if (normalized) {
                    var jobUv = new Jobs.GetUVsUInt8InterleavedNormalizedJob {
                        inputByteStride = (inputByteStride>0) ? inputByteStride : 2,
                        input = (byte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobUv.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var jobUv = new Jobs.GetUVsUInt8InterleavedJob {
                        inputByteStride = (inputByteStride>0) ? inputByteStride : 2,
                        input = (byte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobUv.Schedule(count,GLTFast.DefaultBatchCount);
                }
                break;
            case GLTFComponentType.UnsignedShort:
                if (normalized) {
                    var jobUv = new Jobs.GetUVsUInt16InterleavedNormalizedJob {
                        inputByteStride = (inputByteStride>0) ? inputByteStride : 4,
                        input = (byte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobUv.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var jobUv = new Jobs.GetUVsUInt16InterleavedJob {
                        inputByteStride = (inputByteStride>0) ? inputByteStride : 4,
                        input = (byte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobUv.Schedule(count,GLTFast.DefaultBatchCount);
                }
                break;
            case GLTFComponentType.Short:
                if (normalized) {
                    var job = new Jobs.GetUVsInt16InterleavedNormalizedJob {
                        inputByteStride = inputByteStride > 0 ? inputByteStride : 4,
                        input = (System.Int16*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var job = new Jobs.GetUVsInt16InterleavedJob {
                        inputByteStride = inputByteStride > 0 ? inputByteStride : 4,
                        input = (System.Int16*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = job.Schedule(count,GLTFast.DefaultBatchCount);
                }
                break;
            case GLTFComponentType.Byte:
                var byteStride = inputByteStride>0 ? inputByteStride : 2;
                if (normalized) {
                    var jobInt8 = new Jobs.GetUVsInt8InterleavedNormalizedJob {
                        inputByteStride = inputByteStride > 0 ? inputByteStride : 2,
                        input = (sbyte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobInt8.Schedule(count,GLTFast.DefaultBatchCount);
                } else {
                    var jobInt8 = new Jobs.GetUVsInt8InterleavedJob {
                        inputByteStride = inputByteStride > 0 ? inputByteStride : 2,
                        input = (sbyte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobInt8.Schedule(count,GLTFast.DefaultBatchCount);
                }
                break;
            default:
                Debug.LogErrorFormat( GLTFast.ErrorUnsupportedType, "UV", inputType);
                break;
            }
            Profiler.EndSample();
            return jobHandle;
        }
    }
}
