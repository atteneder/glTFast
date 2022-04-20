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

#if DRACO_UNITY

using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Draco;
using UnityEngine.Rendering;

namespace GLTFast {

    class PrimitiveDracoCreateContext : PrimitiveCreateContextBase {

        DracoMeshLoader draco;
        Task<Mesh> dracoTask;
        Bounds? bounds;

        public override bool IsCompleted => dracoTask!=null && dracoTask.IsCompleted;

        public PrimitiveDracoCreateContext(Bounds? bounds) {
            this.bounds = bounds;
        }

        public void StartDecode(NativeSlice<byte> data, int weightsAttributeId, int jointsAttributeId) {
            draco = new DracoMeshLoader();
            dracoTask = draco.ConvertDracoMeshToUnity(
                data,
                needsNormals,
                needsTangents,
                weightsAttributeId,
                jointsAttributeId,
                morphTargetsContext!=null
                );
        }
        
        public override async Task<Primitive?> CreatePrimitive() {

            var mesh = dracoTask.Result;
            dracoTask.Dispose();

            if (mesh == null) {
                return null;
            }

            if (bounds.HasValue) {
                mesh.bounds = bounds.Value;
                
                // Setting the submeshes' bounds to the overall bounds
                // Calculating the actual sub-mesh bounds (by iterating the verts referenced
                // by the sub-mesh indices) would be slow. Also, hardly any glTFs re-use
                // the same vertex buffer across primitives of a node (which is the
                // only way a mesh can have sub-meshes)
                for (var i = 0; i < mesh.subMeshCount; i++) {
                    var subMeshDescriptor = mesh.GetSubMesh(i);
                    subMeshDescriptor.bounds = bounds.Value;
                    mesh.SetSubMesh(i, subMeshDescriptor, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds );
                }
            } else {
                mesh.RecalculateBounds();
            }
            
            if (morphTargetsContext != null) {
                await morphTargetsContext.ApplyOnMeshAndDispose(mesh);
            }

#if GLTFAST_KEEP_MESH_DATA
            UnityEngine.Profiling.Profiler.BeginSample("UploadMeshData");
            mesh.UploadMeshData(false);
            UnityEngine.Profiling.Profiler.EndSample();
#else
            /// Don't upload explicitely. Unity takes care of upload on demand/deferred

            // Profiler.BeginSample("UploadMeshData");
            // mesh.UploadMeshData(true);
            // Profiler.EndSample();
#endif

            return new Primitive(mesh,materials);
        }
    }
}
#endif // DRACO_UNITY
