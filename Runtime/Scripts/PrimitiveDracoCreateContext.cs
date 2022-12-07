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

        DracoMeshLoader m_Draco;
        Task<Mesh> m_DracoTask;
        Bounds? m_Bounds;

        bool m_NeedsNormals;
        bool m_NeedsTangents;

        public override bool IsCompleted => m_DracoTask!=null && m_DracoTask.IsCompleted;

        public PrimitiveDracoCreateContext(
            int primitiveIndex,
            int materialCount,
            bool needsNormals,
            bool needsTangents,
            string meshName,
            Bounds? bounds
            )
            : base(primitiveIndex, materialCount, meshName)
        {
            m_NeedsNormals = needsNormals;
            m_NeedsTangents = needsTangents;
            m_Bounds = bounds;
        }

        public void StartDecode(NativeSlice<byte> data, int weightsAttributeId, int jointsAttributeId) {
            m_Draco = new DracoMeshLoader();
            m_DracoTask = m_Draco.ConvertDracoMeshToUnity(
                data,
                m_NeedsNormals,
                m_NeedsTangents,
                weightsAttributeId,
                jointsAttributeId,
                morphTargetsContext!=null
                );
        }

        public override async Task<Primitive?> CreatePrimitive() {

            var mesh = m_DracoTask.Result;
            m_DracoTask.Dispose();

            if (mesh == null) {
                return null;
            }

            mesh.name = m_MeshName;

            if (m_Bounds.HasValue) {
                mesh.bounds = m_Bounds.Value;

                // Setting the sub-meshes' bounds to the overall bounds
                // Calculating the actual sub-mesh bounds (by iterating the verts referenced
                // by the sub-mesh indices) would be slow. Also, hardly any glTFs re-use
                // the same vertex buffer across primitives of a node (which is the
                // only way a mesh can have sub-meshes)
                for (var i = 0; i < mesh.subMeshCount; i++) {
                    var subMeshDescriptor = mesh.GetSubMesh(i);
                    subMeshDescriptor.bounds = m_Bounds.Value;
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
            // Don't upload explicitly. Unity takes care of upload on demand/deferred

            // Profiler.BeginSample("UploadMeshData");
            // mesh.UploadMeshData(true);
            // Profiler.EndSample();
#endif

            return new Primitive(mesh,m_Materials);
        }
    }
}
#endif // DRACO_UNITY
