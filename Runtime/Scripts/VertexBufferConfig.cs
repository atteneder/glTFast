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

        public override unsafe JobHandle? Init(VertexInputData posInput) {
            vData = new NativeArray<VType>(posInput.count,Allocator.Persistent);
            var vDataPtr = (byte*) NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(vData);
            JobHandle? posJobHandle;
            fixed( void* input = &(posInput.buffer[posInput.startOffset])) {
                posJobHandle = GetVector3sJob(
                    input,
                    posInput.count,
                    posInput.type,
                    (posInput.byteStride>0) ? posInput.byteStride : 12,
                    (Vector3*) vDataPtr,
                    false // normalized
                );
            }

            return posJobHandle;
        }

        protected void CreateDescriptors() {
            int vadLen = 1;
            vad = new VertexAttributeDescriptor[vadLen];
            var vadCount = 0;
            vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, vadCount);
            vadCount++;
            /*
            if(uvs0.IsCreated) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, vadCount);
                vadCount++;
            }
            if(uvs1.IsCreated) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, vadCount);
                vadCount++;
            }
            if(normals.IsCreated || calculateNormals) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, vadCount);
                vadCount++;
            }
            if(tangents.IsCreated || calculateTangents) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, vadCount);
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

        public override void ApplyOnMesh(UnityEngine.Mesh msh) {

            if (vad == null) {
                CreateDescriptors();
            }

            Profiler.BeginSample("SetVertexBufferParams");
            msh.SetVertexBufferParams(vData.Length,vad);
            Profiler.EndSample();

            MeshUpdateFlags flags = (MeshUpdateFlags)~0;

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
            if(normals.IsCreated) {
                Profiler.BeginSample("SetNormals");
                msh.SetVertexBufferData(normals,0,0,normals.Length,vadCount,flags);
                vadCount++;
                Profiler.EndSample();
            }

            if(tangents.IsCreated) {
                Profiler.BeginSample("SetTangents");
                msh.SetVertexBufferData(tangents,0,0,tangents.Length,vadCount,flags);
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
