#if DEBUG
using System.Collections.Generic;
#endif
using System;
using GLTFast.Vertex;
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
        NativeArray<VType> vData;

        bool hasNormals;
        bool hasTangents;
        bool hasColors;
        
        VertexBufferTexCoordsBase texCoords;
        VertexBufferColors colors;

        public override unsafe JobHandle? ScheduleVertexJobs(
            VertexInputData posInput,
            VertexInputData? nrmInput = null,
            VertexInputData? tanInput = null,
            VertexInputData[] uvInputs = null,
            VertexInputData? colorInput = null
        ) {
            Profiler.BeginSample("ScheduleVertexJobs");
            Profiler.BeginSample("AllocateNativeArray");
            vData = new NativeArray<VType>(posInput.count,defaultAllocator);
            var vDataPtr = (byte*) NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(vData);
            Profiler.EndSample();

            int jobCount = 1;
            int outputByteStride = 12; // sizeof Vector3
            hasNormals = nrmInput.HasValue || calculateNormals; 
            if (hasNormals) {
                if(nrmInput.HasValue) jobCount++;
                outputByteStride += 12;
            }

            hasTangents = tanInput.HasValue || calculateTangents;
            if (hasTangents) {
                if(tanInput.HasValue) jobCount++;
                outputByteStride += 16;
            }
            
            if (uvInputs!=null && uvInputs.Length>0) {
                jobCount += uvInputs.Length;
                switch (uvInputs.Length) {
                    case 1:
                        texCoords = new VertexBufferTexCoords<VTexCoord1>();
                        break;
                    default:
                        texCoords = new VertexBufferTexCoords<VTexCoord2>();
                        break;
                }
            }

            hasColors = colorInput.HasValue;
            if (hasColors) {
                jobCount++;
                colors = new VertexBufferColors();
            }

            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(jobCount, Allocator.Temp);
            int handleIndex = 0;
            
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
                    handles[handleIndex] = h.Value;
                    handleIndex++;
                } else {
                    Profiler.EndSample();
                    return null;
                }
            }

            if (nrmInput.HasValue) {
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
                        handles[handleIndex] = h.Value;
                        handleIndex++;
                    } else {
                        Profiler.EndSample();
                        return null;
                    }
                }
            }
            
            if (tanInput.HasValue) {
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
                        handles[handleIndex] = h.Value;
                        handleIndex++;
                    } else {
                        Profiler.EndSample();
                        return null;
                    }
                }
            }

            if (texCoords!=null) {
                texCoords.ScheduleVertexUVJobs(uvInputs, new NativeSlice<JobHandle>(handles,handleIndex,uvInputs.Length) );
                handleIndex++;
            }
            
            if (hasColors) {
                colors.ScheduleVertexColorJob(colorInput.Value, new NativeSlice<JobHandle>(handles, handleIndex, 1));
                handleIndex++;
            }
            
            var handle = (jobCount > 1) ? JobHandle.CombineDependencies(handles) : handles[0];
            handles.Dispose();
            Profiler.EndSample();
            return handle;
        }

        protected void CreateDescriptors() {
            int vadLen = 1;
            if (hasNormals) vadLen++;
            if (hasTangents) vadLen++;
            if (texCoords != null) vadLen += texCoords.uvSetCount;
            if (colors != null) vadLen++;
            vad = new VertexAttributeDescriptor[vadLen];
            var vadCount = 0;
            int stream = 0;
            vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, stream);
            vadCount++;
            if(hasNormals) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, stream);
                vadCount++;
            }
            if(hasTangents) {
                vad[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, stream);
                vadCount++;
            }
            stream++;
            
            if (texCoords != null) {
                texCoords.AddDescriptors(vad,vadCount,stream);
                vadCount++;
                stream++;
            }

            if (colors != null) {
                colors.AddDescriptors(vad,vadCount,stream);
                vadCount++;
                stream++;
            }
        }

        public override void ApplyOnMesh(UnityEngine.Mesh msh, MeshUpdateFlags flags = MeshUpdateFlags.Default) {

            Profiler.BeginSample("ApplyOnMesh");
            if (vad == null) {
                CreateDescriptors();
            }

            Profiler.BeginSample("SetVertexBufferParams");
            msh.SetVertexBufferParams(vData.Length,vad);
            Profiler.EndSample();

            Profiler.BeginSample("SetVertexBufferData");
            int stream = 0;
            msh.SetVertexBufferData(vData,0,0,vData.Length,stream,flags);
            stream++;
            Profiler.EndSample();

            if (texCoords != null) {
                texCoords.ApplyOnMesh(msh,stream,flags);
                stream++;
            }
            
            if (colors != null) {
                colors.ApplyOnMesh(msh,stream,flags);
                stream++;
            }

            Profiler.EndSample();
        }

        public override void Dispose() {
            if (vData.IsCreated) {
                vData.Dispose();
            }

            if (texCoords != null) {
                texCoords.Dispose();
            }

            if (colors != null) {
                colors.Dispose();
            }
        }
    }
}
