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
using UnityEngine;
using UnityEngine.Profiling;
using Mesh = UnityEngine.Mesh;

namespace GLTFast {

    class MorphTargetsContext {
        MorphTargetContext[] contexts;
        NativeArray<JobHandle> handles;
        int currentIndex;

        public MorphTargetsContext(int morphTargetCount) {
            contexts = new MorphTargetContext[morphTargetCount];
            handles = new NativeArray<JobHandle>(morphTargetCount, VertexBufferConfigBase.defaultAllocator);
            currentIndex = 0;
        }
        
        public bool AddMorphTarget(
            IGltfBuffers buffers,
            int positionAccessorIndex,
            int normalAccessorIndex,
            int tangentAccessorIndex
            )
        {
            var newMorphTarget = new MorphTargetContext();
            var jobHandle = newMorphTarget.ScheduleMorphTargetJobs(
                buffers,
                positionAccessorIndex,
                normalAccessorIndex,
                tangentAccessorIndex
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
                context.AddToMesh(mesh,$"Shape{index}");
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
            int tangentAccessorIndex
        ) {
            Profiler.BeginSample("ScheduleMorphTargetJobs");
            
            buffers.GetAccessor(positionAccessorIndex, out var posAcc, out var posData, out var posByteStride);

            positions = new Vector3[posAcc.count];
            positionsHandle = GCHandle.Alloc(positions,GCHandleType.Pinned);
            
            var jobCount = 1;
            if (normalAccessorIndex >= 0) {
                normals = new Vector3[posAcc.count];
                normalsHandle = GCHandle.Alloc(normals,GCHandleType.Pinned);
                jobCount++;
            }

            if (tangentAccessorIndex >= 0) {
                tangents = new Vector3[posAcc.count];
                tangentsHandle = GCHandle.Alloc(tangents, GCHandleType.Pinned);
                jobCount++;
            }
            
            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(jobCount, VertexBufferConfigBase.defaultAllocator);
            var handleIndex = 0;
            
            fixed( void* dest = &(positions[0])) {
                var h = VertexBufferConfigBase.GetVector3sJob(
                    posData,
                    posAcc.count,
                    posAcc.componentType,
                    posByteStride,
                    (Vector3*) dest,
                    12,
                    posAcc.normalized
                );
                if (h.HasValue) {
                    handles[handleIndex] = h.Value;
                    handleIndex++;
                } else {
                    Profiler.EndSample();
                    return null;
                }
            }

            if (normalAccessorIndex >= 0) {
                buffers.GetAccessor(normalAccessorIndex, out var nrmAcc, out var input, out var inputByteStride);
                fixed( void* dest = &(normals[0])) {
                    var h = VertexBufferConfigBase.GetVector3sJob(
                        input,
                        nrmAcc.count,
                        nrmAcc.componentType,
                        inputByteStride,
                        (Vector3*) dest,
                        12,
                        nrmAcc.normalized
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
            
            if (tangentAccessorIndex >= 0) {
                buffers.GetAccessor(tangentAccessorIndex, out var tanAcc, out var input, out var inputByteStride);
                fixed( void* dest = &(tangents[0])) {
                    var h = VertexBufferConfigBase.GetVector3sJob(
                        input,
                        tanAcc.count,
                        tanAcc.componentType,
                        inputByteStride,
                        (Vector3*) dest,
                        12,
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
