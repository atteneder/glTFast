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
using GLTFast.Vertex;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

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
            VertexInputData posInput,
            VertexInputData? nrmInput = null,
            VertexInputData? tanInput = null
            )
        {
            var newMorphTarget = new MorphTargetContext();
            var jobHandle = newMorphTarget.ScheduleMorphTargetJobs(posInput, nrmInput, tanInput);
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
            VertexInputData posInput,
            VertexInputData? nrmInput = null,
            VertexInputData? tanInput = null
        ) {
            Profiler.BeginSample("ScheduleMorphTargetJobs");
            
            positions = new Vector3[posInput.count];
            positionsHandle = GCHandle.Alloc(positions,GCHandleType.Pinned);
            
            var jobCount = 1;
            if (nrmInput.HasValue) {
                normals = new Vector3[nrmInput.Value.count];
                normalsHandle = GCHandle.Alloc(normals,GCHandleType.Pinned);
                jobCount++;
            }

            if (tanInput.HasValue) {
                tangents = new Vector3[tanInput.Value.count];
                tangentsHandle = GCHandle.Alloc(tangents, GCHandleType.Pinned);
                jobCount++;
            }
            
            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(jobCount, VertexBufferConfigBase.defaultAllocator);
            var handleIndex = 0;
            
            fixed( void* input = &(posInput.buffer[posInput.startOffset]), dest = &(positions[0])) {
                var h = VertexBufferConfigBase.GetVector3sJob(
                    input,
                    posInput.count,
                    posInput.type,
                    posInput.byteStride,
                    (Vector3*) dest,
                    12,
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
                fixed( void* input = &(nrmInput.Value.buffer[nrmInput.Value.startOffset]), dest = &(normals[0])) {
                    var h = VertexBufferConfigBase.GetVector3sJob(
                        input,
                        nrmInput.Value.count,
                        nrmInput.Value.type,
                        nrmInput.Value.byteStride,
                        (Vector3*) dest,
                        12,
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
                fixed( void* input = &(tanInput.Value.buffer[tanInput.Value.startOffset]), dest = &(tangents[0])) {
                    var h = VertexBufferConfigBase.GetVector3sJob(
                        input,
                        tanInput.Value.count,
                        tanInput.Value.type,
                        tanInput.Value.byteStride,
                        (Vector3*) dest,
                        12,
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
