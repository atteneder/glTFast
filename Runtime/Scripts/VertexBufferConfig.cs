// Copyright 2020-2022 Andreas Atteneder
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

#if DEBUG
using System.Collections.Generic;
#endif
using System;
using GLTFast.Vertex;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace GLTFast
{
#if BURST
    using Unity.Mathematics;
#endif
    using Logging;

    class VertexBufferConfig<VType> :
        VertexBufferConfigBase
        where VType : struct
    {
        NativeArray<VType> vData;

        bool hasNormals;
        bool hasTangents;
        bool hasColors;
        bool hasBones;
        
        VertexBufferTexCoordsBase texCoords;
        VertexBufferColors colors;
        VertexBufferBones bones;

        public override int vertexCount {
            get {
                if (vData.IsCreated) {
                    return vData.Length;
                }
                return 0;
            }
        }

        public VertexBufferConfig(ICodeLogger logger) : base(logger) {}

        public override unsafe JobHandle? ScheduleVertexJobs(
            IGltfBuffers buffers,
            int positionAccessorIndex,
            int normalAccessorIndex,
            int tangentAccessorIndex,
            int[] uvAccessorIndices,
            int colorAccessorIndex,
            int weightsAccessorIndex,
            int jointsAccessorIndex
        ) {
            buffers.GetAccessor(positionAccessorIndex, out var posAcc, out var posData, out var posByteStride);
            
            Profiler.BeginSample("ScheduleVertexJobs");
            Profiler.BeginSample("AllocateNativeArray");
            vData = new NativeArray<VType>(posAcc.count,defaultAllocator);
            var vDataPtr = (byte*) vData.GetUnsafeReadOnlyPtr();
            Profiler.EndSample();

            bounds = posAcc.TryGetBounds();
            
            int jobCount = 1;
            int outputByteStride = 12; // sizeof Vector3
            if (posAcc.isSparse && posAcc.bufferView>=0) {
                jobCount++;
            }
            if (normalAccessorIndex>=0) {
                jobCount++;
                hasNormals = true;
            }
            hasNormals |= calculateNormals;
            if (hasNormals) {
                outputByteStride += 12;
            }

            if (tangentAccessorIndex>=0) {
                jobCount++;
                hasTangents = true;
            }
            hasTangents |= calculateTangents;
            if (hasTangents) {
                outputByteStride += 16;
            }
            
            if (uvAccessorIndices!=null && uvAccessorIndices.Length>0) {
                
                // More than two UV sets are not supported yet
                Assert.IsTrue(uvAccessorIndices.Length<9);
                
                jobCount += uvAccessorIndices.Length;
                switch (uvAccessorIndices.Length) {
                    case 1:
                        texCoords = new VertexBufferTexCoords<VTexCoord1>(logger);
                        break;
                    case 2:
                        texCoords = new VertexBufferTexCoords<VTexCoord2>(logger);
                        break;
                    case 3:
                        texCoords = new VertexBufferTexCoords<VTexCoord3>(logger);
                        break;
                    case 4:
                        texCoords = new VertexBufferTexCoords<VTexCoord4>(logger);
                        break;
                    case 5:
                        texCoords = new VertexBufferTexCoords<VTexCoord5>(logger);
                        break;
                    case 6:
                        texCoords = new VertexBufferTexCoords<VTexCoord6>(logger);
                        break;
                    case 7:
                        texCoords = new VertexBufferTexCoords<VTexCoord7>(logger);
                        break;
                    default:
                        texCoords = new VertexBufferTexCoords<VTexCoord8>(logger);
                        break;
                }
            }

            hasColors = colorAccessorIndex >= 0;
            if (hasColors) {
                jobCount++;
                colors = new VertexBufferColors();
            }

            hasBones = weightsAccessorIndex >= 0 && jointsAccessorIndex >= 0;
            if(hasBones) {
                jobCount++;
                bones = new VertexBufferBones(logger);
            }

            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(jobCount, defaultAllocator);
            int handleIndex = 0;
            
            {
                JobHandle? h = null;
                if(posAcc.bufferView>=0) {
                    h = GetVector3sJob(
                        posData,
                        posAcc.count,
                        posAcc.componentType,
                        posByteStride,
                        (float3*) vDataPtr,
                        outputByteStride,
                        posAcc.normalized,
                        false // positional data never needs to be normalized
                    );
                }
                if (posAcc.isSparse) {
                    buffers.GetAccessorSparseIndices(posAcc.sparse.indices, out var posIndexData);
                    buffers.GetAccessorSparseValues(posAcc.sparse.values, out var posValueData);
                    var sparseJobHandle = GetVector3sSparseJob(
                        posIndexData,
                        posValueData,
                        posAcc.sparse.count,
                        posAcc.sparse.indices.componentType,
                        posAcc.componentType,
                        (float3*) vDataPtr,
                        outputByteStride,
                        dependsOn: ref h,
                        posAcc.normalized
                    );
                    if (sparseJobHandle.HasValue) {
                        handles[handleIndex] = sparseJobHandle.Value;
                        handleIndex++;
                    } else {
                        Profiler.EndSample();
                        return null;
                    }
                }
                if (h.HasValue) {
                    handles[handleIndex] = h.Value;
                    handleIndex++;
                } else {
                    Profiler.EndSample();
                    return null;
                }
            }

            if (normalAccessorIndex>=0) {
                buffers.GetAccessor(normalAccessorIndex, out var nrmAcc, out var input, out var inputByteStride);
                if (nrmAcc.isSparse) {
                    logger.Error(LogCode.SparseAccessor,"normals");
                }
                var h = GetVector3sJob(
                    input,
                    nrmAcc.count,
                    nrmAcc.componentType,
                    inputByteStride,
                    (float3*) (vDataPtr+12),
                    outputByteStride,
                    nrmAcc.normalized,
                    true // normals need to be unit length
                );
                if (h.HasValue) {
                    handles[handleIndex] = h.Value;
                    handleIndex++;
                } else {
                    Profiler.EndSample();
                    return null;
                }
            }
            
            if (tangentAccessorIndex>=0) {
                buffers.GetAccessor(tangentAccessorIndex, out var tanAcc, out var input, out var inputByteStride);
                if (tanAcc.isSparse) {
                    logger.Error(LogCode.SparseAccessor,"tangents");
                }
                var h = GetTangentsJob(
                    input,
                    tanAcc.count,
                    tanAcc.componentType,
                    inputByteStride,
                    (float4*) (vDataPtr+24),
                    outputByteStride,
                    tanAcc.normalized
                );
                if (h.HasValue) {
                    handles[handleIndex] = h.Value;
                    handleIndex++;
                } else {
                    Profiler.EndSample();
                    return null;
                }
            }

            if (texCoords!=null) {
                texCoords.ScheduleVertexUVJobs(
                    buffers,
                    uvAccessorIndices,
                    posAcc.count,
                    new NativeSlice<JobHandle>(
                        handles,
                        handleIndex,
                        uvAccessorIndices.Length
                        )
                    );
                handleIndex += uvAccessorIndices.Length;
            }
            
            if (hasColors) {
                colors.ScheduleVertexColorJob(
                    buffers,
                    colorAccessorIndex,
                    new NativeSlice<JobHandle>(
                        handles, 
                        handleIndex, 
                        1
                        )
                    );
                handleIndex++;
            }

            if (hasBones) {
                var h = bones.ScheduleVertexBonesJob(
                    buffers,
                    weightsAccessorIndex,
                    jointsAccessorIndex
                );
                if (h.HasValue) {
                    handles[handleIndex] = h.Value;
                    handleIndex++;
                } else {
                    Profiler.EndSample();
                    return null;
                }
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
            if (bones != null) vadLen+=2;
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

            if (colors != null) {
                colors.AddDescriptors(vad,vadCount,stream);
                vadCount++;
                stream++;
            }
            
            if (texCoords != null) {
                texCoords.AddDescriptors(vad,ref vadCount,stream);
                stream++;
            }

            if (bones != null) {
                bones.AddDescriptors(vad,vadCount,stream);
                vadCount+=2;
                stream++;
            }
        }

        public override void ApplyOnMesh(UnityEngine.Mesh msh, MeshUpdateFlags flags = PrimitiveCreateContextBase.defaultMeshUpdateFlags) {

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

            if (colors != null) {
                colors.ApplyOnMesh(msh,stream,flags);
                stream++;
            }
            
            if (texCoords != null) {
                texCoords.ApplyOnMesh(msh,stream,flags);
                stream++;
            }
            
            if (bones != null) {
                bones.ApplyOnMesh(msh,stream,flags);
                stream++;
            }

            Profiler.EndSample();
        }

        public override void Dispose() {
            if (vData.IsCreated) {
                vData.Dispose();
            }

            if (colors != null) {
                colors.Dispose();
            }

            if (texCoords != null) {
                texCoords.Dispose();
            }

            if (bones != null) {
                bones.Dispose();
            }
        }
    }
}
