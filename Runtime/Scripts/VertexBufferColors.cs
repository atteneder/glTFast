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

    abstract class VertexBufferColorsBase {
        public abstract unsafe bool ScheduleVertexColorJob(VertexInputData colorInput, NativeSlice<JobHandle> handles);
        public abstract void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream);
        public abstract void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = MeshUpdateFlags.Default);
        public abstract void Dispose();
    }

    class VertexBufferColors : VertexBufferColorsBase {
        NativeArray<Color> vData;

        public override unsafe bool ScheduleVertexColorJob(VertexInputData colorInput, NativeSlice<JobHandle> handles) {
            Profiler.BeginSample("ScheduleVertexColorJob");
            Profiler.BeginSample("AllocateNativeArray");
            vData = new NativeArray<Color>(colorInput.count, VertexBufferConfigBase.defaultAllocator);
            var vDataPtr = (byte*) NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(vData);
            Profiler.EndSample();

            fixed( void* input = &(colorInput.buffer[colorInput.startOffset])) {
                var h = GetColors32Job(
                    input,
                    colorInput.type,
                    colorInput.attributeType,
                    colorInput.byteStride,
                    vData
                );
                if (h.HasValue) {
                    handles[0] = h.Value;
                } else {
                    Profiler.EndSample();
                    return false;
                }
            }
            Profiler.EndSample();
            return true;
        }

        public override void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream) {
            dst[offset] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, stream);
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
        
        unsafe JobHandle? GetColors32Job(
            void* input,
            GLTFComponentType inputType,
            GLTFAccessorAttributeType attributeType,
            int inputByteStride,
            NativeArray<Color> output
            )
        {
            Profiler.BeginSample("PrepareColors32");
            JobHandle? jobHandle = null;

            if (attributeType == GLTFAccessorAttributeType.VEC3)
            {
                switch (inputType)
                {
                    case GLTFComponentType.UnsignedByte:
                        {
                            var job = new Jobs.GetColorsVec3UInt8Job {
                                input = (byte*) input,
                                inputByteStride = inputByteStride>0 ? inputByteStride : 3,
                                result = output
                            };
                            jobHandle = job.Schedule(output.Length,GLTFast.DefaultBatchCount);
                        }
                        break;
                    case GLTFComponentType.Float:
                        {
                            var job = new Jobs.GetColorsVec3FloatJob {
                                input = (float*) input,
                                inputByteStride = inputByteStride>0 ? inputByteStride : 12,
                                result = output
                            };
                            jobHandle = job.Schedule(output.Length,GLTFast.DefaultBatchCount);
                        }
                        break;
                    case GLTFComponentType.UnsignedShort:
                        {
                            var job = new Jobs.GetColorsVec3UInt16Job {
                                input = (System.UInt16*)input,
                                inputByteStride = inputByteStride>0 ? inputByteStride : 6,
                                result = output
                            };
                            jobHandle = job.Schedule(output.Length,GLTFast.DefaultBatchCount);
                        }
                        break;
                    default:
                        Debug.LogErrorFormat(GLTFast.ErrorUnsupportedColorFormat, attributeType);
                        break;
                }
            }
            else if (attributeType == GLTFAccessorAttributeType.VEC4)
            {
                switch (inputType)
                {
                    case GLTFComponentType.UnsignedByte:
                        {
                            var job = new Jobs.GetColorsVec4UInt8Job {
                                input = (byte*) input,
                                inputByteStride = inputByteStride > 0 ? inputByteStride : 4,
                                result = output
                            };
                            jobHandle = job.Schedule(output.Length,GLTFast.DefaultBatchCount);
                        }
                        break;
                    case GLTFComponentType.Float:
                        {
                            var job = new Jobs.MemCopyJob();
                            job.bufferSize = output.Length*16;
                            job.input = input;
                            job.result = NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(output);
                            jobHandle = job.Schedule();
                        }
                        break;
                    case GLTFComponentType.UnsignedShort:
                        {
                            var job = new Jobs.GetColorsVec4UInt16Job {
                                input = (System.UInt16*) input,
                                inputByteStride = inputByteStride>0 ? inputByteStride : 8,
                                result = output
                            };
                            jobHandle = job.Schedule(output.Length,GLTFast.DefaultBatchCount);
                        }
                        break;
                    default:
                        Debug.LogErrorFormat(GLTFast.ErrorUnsupportedColorFormat, attributeType);
                        break;
                }
            } else {
                Debug.LogErrorFormat(GLTFast.ErrorUnsupportedType, "color accessor", inputType);
            }
            Profiler.EndSample();
            return jobHandle;
        }
    }
}
