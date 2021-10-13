// Copyright 2020-2021 Andreas Atteneder
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
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using Mesh = UnityEngine.Mesh;

namespace GLTFast {

    class MorphTargetsContext {
        MorphTargetContext[] contexts;
        NativeArray<JobHandle> handles;
        int currentIndex;
        string[] meshTargetNames;

        public MorphTargetsContext(int morphTargetCount, string[] meshTargetNames) {
            contexts = new MorphTargetContext[morphTargetCount];
            handles = new NativeArray<JobHandle>(morphTargetCount, VertexBufferConfigBase.defaultAllocator);
            currentIndex = 0;
            this.meshTargetNames = meshTargetNames;
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
                handles[currentIndex] = jobHandle.Value;
                contexts[currentIndex] = newMorphTarget;
                currentIndex++;
            }
            else {
                return false;
            }
            return true;
        }

        public JobHandle GetJobHandle() {
            var handle = (contexts.Length > 1) ? JobHandle.CombineDependencies(handles) : handles[0];
            handles.Dispose();
            return handle;
        }

        public void ApplyOnMeshAndDispose(Mesh mesh) {
            for (var index = 0; index < contexts.Length; index++) {
                var context = contexts[index];
                context.AddToMesh(mesh, meshTargetNames?[index] ?? index.ToString());
                context.Dispose();
            }
            contexts = null;
        }
    }

    class MorphTargetContext {

        Vector3[] positions;
        Vector3[] normals;
        Vector3[] tangents;

        GCHandle positionsHandle;
        GCHandle normalsHandle;
        GCHandle tangentsHandle;

        public unsafe JobHandle? ScheduleMorphTargetJobs(
            IGltfBuffers buffers,
            int positionAccessorIndex,
            int normalAccessorIndex,
            int tangentAccessorIndex,
            ICodeLogger logger
        ) {
            Profiler.BeginSample("ScheduleMorphTargetJobs");
            
            buffers.GetAccessor(positionAccessorIndex, out var posAcc, out var posData, out var posByteStride);
            
            positions = new Vector3[posAcc.count];
            positionsHandle = GCHandle.Alloc(positions,GCHandleType.Pinned);
            
            var jobCount = 1;
            if (posAcc.isSparse && posAcc.bufferView>=0) {
                jobCount++;
            }

            Accessor nrmAcc = null;
            void* nrmInput = null;
            int nrmInputByteStride = 0;
            
            if (normalAccessorIndex >= 0) {
                normals = new Vector3[posAcc.count];
                normalsHandle = GCHandle.Alloc(normals,GCHandleType.Pinned);
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
                tangents = new Vector3[posAcc.count];
                tangentsHandle = GCHandle.Alloc(tangents, GCHandleType.Pinned);
                buffers.GetAccessor(normalAccessorIndex, out tanAcc, out tanInput, out tanInputByteStride);
                if (tanAcc.isSparse && tanAcc.bufferView>=0) {
                    jobCount+=2;
                } else {
                    jobCount++;
                }
            }
            
            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(jobCount, VertexBufferConfigBase.defaultAllocator);
            var handleIndex = 0;
            
            fixed (void* dest = &(positions[0])) {
                JobHandle? h = null;
                if (posData!=null) {
#if DEBUG
                    if (posAcc.normalized) {
                        Debug.LogError("Normalized Positions will likely produce incorrect results. Please report this error at https://github.com/atteneder/glTFast/issues/new?assignees=&labels=bug&template=bug_report.md&title=Normalized%20Positions");
                    }
#endif
                    h = VertexBufferConfigBase.GetVector3sJob(
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
                    var sparseJobHandle = VertexBufferConfigBase.GetVector3sSparseJob(
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
                fixed( void* dest = &(normals[0])) {
                    JobHandle? h = null;
                    if (nrmAcc.bufferView >= 0) {
                        h = VertexBufferConfigBase.GetVector3sJob(
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
                        var sparseJobHandle = VertexBufferConfigBase.GetVector3sSparseJob(
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
                fixed( void* dest = &(tangents[0])) {
                    JobHandle? h = null;
                    if (tanAcc.bufferView >= 0) {
                        h = VertexBufferConfigBase.GetVector3sJob(
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
                        var sparseJobHandle = VertexBufferConfigBase.GetVector3sSparseJob(
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
                            handleIndex++;
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
            mesh.AddBlendShapeFrame(name,1f,positions,normals,tangents);
            Profiler.EndSample();
        }

        public void Dispose() {
            positionsHandle.Free();
            positions = null;
            if (normals != null) {
                normalsHandle.Free();
                normals = null;
            }
            if (tangents != null) {
                tangentsHandle.Free();
                tangents = null;
            }
        }
    }
}
