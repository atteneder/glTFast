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

    class VertexBufferConfig<VType> :
        VertexBufferConfigBase
        where VType : struct
    {
        public VertexBufferConfig() {
#if DEBUG
            meshIndices = new HashSet<int>();
#endif
        }

        NativeArray<VType> vData;
        bool hasNormals;
        bool hasTangents;

        public override unsafe JobHandle? Init(
            VertexInputData posInput,
            VertexInputData? nrmInput = null,
            VertexInputData? tanInput = null
        ) {
            vData = new NativeArray<VType>(posInput.count,Allocator.Persistent);
            var vDataPtr = (byte*) NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(vData);

            int jobCount = 1;
            int outputByteStride = 12; // sizeof Vector3
            hasNormals = nrmInput.HasValue;// || calculateNormals; 
            if (hasNormals) {
                jobCount++;
                outputByteStride += 12;
            }

            hasTangents = tanInput.HasValue; //  || calculateTangents;
            if (hasTangents) {
                jobCount++;
                outputByteStride += 16;
            }
            var handles = new JobHandle[jobCount];

            fixed( void* input = &(posInput.buffer[posInput.startOffset])) {
                var h = GetVector3sJob(
                    input,
                    posInput.count,
                    posInput.type,
                    posInput.byteStride,
                    (Vector3*) vDataPtr,
                    outputByteStride,
                    posInput.normalize
                );
                if (h.HasValue) {
                    handles[0] = h.Value;
                } else {
                    return null;
                }
            }

            if (hasNormals) {
                fixed( void* input = &(nrmInput.Value.buffer[nrmInput.Value.startOffset])) {
                    var h = GetVector3sJob(
                        input,
                        nrmInput.Value.count,
                        nrmInput.Value.type,
                        nrmInput.Value.byteStride,
                        (Vector3*) (vDataPtr+12),
                        outputByteStride,
                        nrmInput.Value.normalize
                    );
                    if (h.HasValue) {
                        handles[1] = h.Value;
                    } else {
                        return null;
                    }
                }
            }
            
            if (hasTangents) {
                fixed( void* input = &(tanInput.Value.buffer[tanInput.Value.startOffset])) {
                    var h = GetTangentsJob(
                        input,
                        tanInput.Value.count,
                        tanInput.Value.type,
                        tanInput.Value.byteStride,
                        (Vector4*) (vDataPtr+24),
                        outputByteStride,
                        tanInput.Value.normalize
                    );
                    if (h.HasValue) {
                        handles[2] = h.Value;
                    } else {
                        return null;
                    }
                }
            }

            if (jobCount > 1) {
                NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(handles, Allocator.Temp);
                var handle = JobHandle.CombineDependencies(jobHandles);
                jobHandles.Dispose();
                return handle;
            }
            return handles[0];
        }

        protected void CreateDescriptors() {
            int vadLen = 1;
            if (hasNormals) vadLen++;
            if (hasTangents) vadLen++;
            vad = new VertexAttributeDescriptor[vadLen];
            var vadCount = 0;
            vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0);
            vadCount++;
            if(hasNormals) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 0);
                vadCount++;
            }
            if(hasTangents) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 0);
                vadCount++;
            }
            /*
            if(uvs0.IsCreated) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, vadCount);
                vadCount++;
            }
            if(uvs1.IsCreated) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, vadCount);
                vadCount++;
            }
            if(colors32.IsCreated) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4, vadCount);
                vadCount++;
            } else
            if(colors.IsCreated) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, vadCount);
                vadCount++;
            }
            */
        }

        public override void ApplyOnMesh(UnityEngine.Mesh msh, MeshUpdateFlags flags = MeshUpdateFlags.Default) {

            if (vad == null) {
                CreateDescriptors();
            }

            Profiler.BeginSample("SetVertexBufferParams");
            msh.SetVertexBufferParams(vData.Length,vad);
            Profiler.EndSample();

            Profiler.BeginSample("SetVertexBufferData");
            int vadCount = 0;
            msh.SetVertexBufferData(vData,0,0,vData.Length,vadCount,flags);
            vadCount++;
            Profiler.EndSample();

            /*
            if(uvs0.IsCreated) {
                Profiler.BeginSample("SetUVs0");
                msh.SetVertexBufferData(uvs0,0,0,uvs0.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            }
            if(uvs1.IsCreated) {
                Profiler.BeginSample("SetUVs1");
                msh.SetVertexBufferData(uvs1,0,0,uvs1.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            }
            if(colors32.IsCreated) {
                Profiler.BeginSample("SetColors32");
                msh.SetVertexBufferData(colors32,0,0,colors32.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            } else
            if(colors.IsCreated) {
                Profiler.BeginSample("SetColors");
                msh.SetVertexBufferData(colors,0,0,colors.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            }
            //*/
        }

        public override void Dispose() {
            if (vData.IsCreated) {
                vData.Dispose();
            } 
        }
    }
}
