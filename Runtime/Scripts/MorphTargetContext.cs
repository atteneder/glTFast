﻿// Copyright 2020-2022 Andreas Atteneder
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

using System.Runtime.InteropServices;
using GLTFast.Schema;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Mesh = UnityEngine.Mesh;
using System.Threading.Tasks;
using GLTFast.Logging;

namespace GLTFast {

    class MorphTargetsContext {
        MorphTargetContext[] m_Contexts;
        NativeArray<JobHandle> m_Handles;
        int m_CurrentIndex;
        string[] m_MeshTargetNames;
        IDeferAgent m_DeferAgent;

        public MorphTargetsContext(int morphTargetCount, string[] meshTargetNames, IDeferAgent deferAgent) {
            m_Contexts = new MorphTargetContext[morphTargetCount];
            m_Handles = new NativeArray<JobHandle>(morphTargetCount, VertexBufferConfigBase.defaultAllocator);
            m_CurrentIndex = 0;
            this.m_MeshTargetNames = meshTargetNames;
            this.m_DeferAgent = deferAgent;
        }
        
        public bool AddMorphTarget(
            IGltfBuffers buffers,
            int positionAccessorIndex,
            int normalAccessorIndex,
            int tangentAccessorIndex,
            ICodeLogger logger
            )
        {
            var newMorphTarget = new MorphTargetContext();
            var jobHandle = newMorphTarget.ScheduleMorphTargetJobs(
                buffers,
                positionAccessorIndex,
                normalAccessorIndex,
                tangentAccessorIndex,
                logger
                );
            if (jobHandle.HasValue) {
                m_Handles[m_CurrentIndex] = jobHandle.Value;
                m_Contexts[m_CurrentIndex] = newMorphTarget;
                m_CurrentIndex++;
            }
            else {
                return false;
            }
            return true;
        }

        public JobHandle GetJobHandle() {
            var handle = (m_Contexts.Length > 1) ? JobHandle.CombineDependencies(m_Handles) : m_Handles[0];
            m_Handles.Dispose();
            return handle;
        }

        public async Task ApplyOnMeshAndDispose(Mesh mesh) {
            for (var index = 0; index < m_Contexts.Length; index++) {
                var context = m_Contexts[index];
                context.AddToMesh(mesh, m_MeshTargetNames?[index] ?? index.ToString());
                context.Dispose();
                await m_DeferAgent.BreakPoint();
            }
            m_Contexts = null;
        }
    }

    class MorphTargetContext {

        Vector3[] m_Positions;
        Vector3[] m_Normals;
        Vector3[] m_Tangents;

        GCHandle m_PositionsHandle;
        GCHandle m_NormalsHandle;
        GCHandle m_TangentsHandle;

        public unsafe JobHandle? ScheduleMorphTargetJobs(
            IGltfBuffers buffers,
            int positionAccessorIndex,
            int normalAccessorIndex,
            int tangentAccessorIndex,
            ICodeLogger logger
        ) {
            Profiler.BeginSample("ScheduleMorphTargetJobs");
            
            buffers.GetAccessor(positionAccessorIndex, out var posAcc, out var posData, out var posByteStride);
            
            m_Positions = new Vector3[posAcc.count];
            m_PositionsHandle = GCHandle.Alloc(m_Positions,GCHandleType.Pinned);
            
            var jobCount = 1;
            if (posAcc.isSparse && posAcc.bufferView>=0) {
                jobCount++;
            }

            Accessor nrmAcc = null;
            void* nrmInput = null;
            int nrmInputByteStride = 0;
            
            if (normalAccessorIndex >= 0) {
                m_Normals = new Vector3[posAcc.count];
                m_NormalsHandle = GCHandle.Alloc(m_Normals,GCHandleType.Pinned);
                buffers.GetAccessor(normalAccessorIndex, out nrmAcc, out nrmInput, out nrmInputByteStride);
                if (nrmAcc.isSparse && nrmAcc.bufferView>=0) {
                    jobCount+=2;
                } else {
                    jobCount++;
                }
            }

            Accessor tanAcc = null;
            void* tanInput = null;
            int tanInputByteStride = 0;
            
            if (tangentAccessorIndex >= 0) {
                m_Tangents = new Vector3[posAcc.count];
                m_TangentsHandle = GCHandle.Alloc(m_Tangents, GCHandleType.Pinned);
                buffers.GetAccessor(normalAccessorIndex, out tanAcc, out tanInput, out tanInputByteStride);
                if (tanAcc.isSparse && tanAcc.bufferView>=0) {
                    jobCount+=2;
                } else {
                    jobCount++;
                }
            }
            
            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(jobCount, VertexBufferConfigBase.defaultAllocator);
            var handleIndex = 0;
            
            fixed (void* dest = &(m_Positions[0])) {
                JobHandle? h = null;
                if (posData!=null) {
                    h = VertexBufferConfigBase.GetVector3Job(
                        posData,
                        posAcc.count,
                        posAcc.componentType,
                        posByteStride,
                        (float3*)dest,
                        12,
                        posAcc.normalized,
                        false // positional data never needs to be normalized
                    );
                    if (h.HasValue) {
                        handles[handleIndex] = h.Value;
                        handleIndex++;
                    }
                    else {
                        Profiler.EndSample();
                        return null;
                    }
                }
                if (posAcc.isSparse) {
                    buffers.GetAccessorSparseIndices(posAcc.sparse.indices, out var posIndexData);
                    buffers.GetAccessorSparseValues(posAcc.sparse.values, out var posValueData);
                    var sparseJobHandle = VertexBufferConfigBase.GetVector3SparseJob(
                        posIndexData,
                        posValueData,
                        posAcc.sparse.count,
                        posAcc.sparse.indices.componentType,
                        posAcc.componentType,
                        (float3*) dest,
                        12,
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
            }

            if (nrmAcc!=null) {
                fixed( void* dest = &(m_Normals[0])) {
                    JobHandle? h = null;
                    if (nrmAcc.bufferView >= 0) {
                        h = VertexBufferConfigBase.GetVector3Job(
                            nrmInput,
                            nrmAcc.count,
                            nrmAcc.componentType,
                            nrmInputByteStride,
                            (float3*)dest,
                            12,
                            nrmAcc.normalized,
                            false // morph target normals are deltas -> don't normalize
                        );
                        if (h.HasValue) {
                            handles[handleIndex] = h.Value;
                            handleIndex++;
                        }
                        else {
                            Profiler.EndSample();
                            return null;
                        }
                    }
                    if (nrmAcc.isSparse) {
                        buffers.GetAccessorSparseIndices(nrmAcc.sparse.indices, out var indexData);
                        buffers.GetAccessorSparseValues(nrmAcc.sparse.values, out var valueData);
                        var sparseJobHandle = VertexBufferConfigBase.GetVector3SparseJob(
                            indexData,
                            valueData,
                            nrmAcc.sparse.count,
                            nrmAcc.sparse.indices.componentType,
                            nrmAcc.componentType,
                            (float3*) dest,
                            12,
                            dependsOn: ref h,
                            nrmAcc.normalized
                        );
                        if (sparseJobHandle.HasValue) {
                            handles[handleIndex] = sparseJobHandle.Value;
                            handleIndex++;
                        } else {
                            Profiler.EndSample();
                            return null;
                        }
                    }
                }
            }
            
            if (tanAcc!=null) {
                fixed( void* dest = &(m_Tangents[0])) {
                    JobHandle? h = null;
                    if (tanAcc.bufferView >= 0) {
                        h = VertexBufferConfigBase.GetVector3Job(
                            tanInput,
                            tanAcc.count,
                            tanAcc.componentType,
                            tanInputByteStride,
                            (float3*)dest,
                            12,
                            tanAcc.normalized,
                            false // morph target tangents are deltas -> don't normalize
                        );
                        if (h.HasValue) {
                            handles[handleIndex] = h.Value;
                            handleIndex++;
                        }
                        else {
                            Profiler.EndSample();
                            return null;
                        }
                    }
                    if (tanAcc.isSparse) {
                        buffers.GetAccessorSparseIndices(tanAcc.sparse.indices, out var indexData);
                        buffers.GetAccessorSparseValues(tanAcc.sparse.values, out var valueData);
                        var sparseJobHandle = VertexBufferConfigBase.GetVector3SparseJob(
                            indexData,
                            valueData,
                            tanAcc.sparse.count,
                            tanAcc.sparse.indices.componentType,
                            tanAcc.componentType,
                            (float3*) dest,
                            12,
                            dependsOn: ref h,
                            tanAcc.normalized
                        );
                        if (sparseJobHandle.HasValue) {
                            handles[handleIndex] = sparseJobHandle.Value;
                        } else {
                            Profiler.EndSample();
                            return null;
                        }
                    }
                }
            }

            var handle = (jobCount > 1) ? JobHandle.CombineDependencies(handles) : handles[0];
            handles.Dispose();
            Profiler.EndSample();
            return handle;
        }

        public void AddToMesh(Mesh mesh, string name) {
            Profiler.BeginSample("AddBlendShapeFrame");
            mesh.AddBlendShapeFrame(name,1f,m_Positions,m_Normals,m_Tangents);
            Profiler.EndSample();
        }

        public void Dispose() {
            m_PositionsHandle.Free();
            m_Positions = null;
            if (m_Normals != null) {
                m_NormalsHandle.Free();
                m_Normals = null;
            }
            if (m_Tangents != null) {
                m_TangentsHandle.Free();
                m_Tangents = null;
            }
        }
    }
}
